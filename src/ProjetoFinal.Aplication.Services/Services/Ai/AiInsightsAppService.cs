using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ProjetoFinal.Application.Contracts.Dto.Ai;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

namespace ProjetoFinal.Aplication.Services.Services.Ai;

public class AiInsightsAppService : IAiInsightsAppService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly AiProviderConfiguration _configuration;
    private readonly ICourseContentRepository _courseContentRepository;
    private readonly IForumPostRepository _forumPostRepository;

    public AiInsightsAppService(
        HttpClient httpClient,
        IOptions<AiProviderConfiguration> configuration,
        ICourseContentRepository courseContentRepository,
        IForumPostRepository forumPostRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration.Value;
        _courseContentRepository = courseContentRepository;
        _forumPostRepository = forumPostRepository;
    }

    public async Task<AiContentSummaryDto> GenerateContentSummaryAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        EnsureConfiguration();

        var content = await _courseContentRepository.GetByIdAsync(contentId, cancellationToken);
        if (content is null)
        {
            throw new BusinessException("Conteudo nao encontrado para resumo.", ECodigo.NaoEncontrado);
        }

        var sourceText = BuildContentSource(content.Title, content.Summary, content.Body);
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            throw new BusinessException("O conteudo nao possui texto suficiente para gerar resumo.", ECodigo.NaoPermitido);
        }

        var prompt = $$"""
        Voce e um assistente educacional em portugues do Brasil.
        Gere um resumo claro e didatico para estudantes.
        Responda exclusivamente em JSON valido no formato:
        {
          "summary": "texto curto com no maximo 120 palavras",
          "keyPoints": ["ponto 1", "ponto 2", "ponto 3"],
          "attentionPoints": ["atencao 1", "atencao 2"]
        }

        Titulo: {{content.Title}}
        Resumo original: {{content.Summary ?? "Nao informado"}}
        Conteudo:
        {{sourceText}}
        """;

        var completion = await CreateCompletionAsync(prompt, cancellationToken);
        var parsed = DeserializeOrThrow<AiContentSummaryPayload>(completion);

        return new AiContentSummaryDto
        {
            Summary = parsed.Summary?.Trim() ?? string.Empty,
            KeyPoints = SanitizeList(parsed.KeyPoints, 5),
            AttentionPoints = SanitizeList(parsed.AttentionPoints, 4),
            Model = _configuration.Model,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<AiInstructorFrequentQuestionsDto> GetInstructorFrequentQuestionsAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        EnsureConfiguration();

        var posts = await _forumPostRepository.GetAllAsync(new ForumPostFilter
        {
            InstructorId = instructorId,
            PageNumber = 1,
            PageSize = 200
        }, cancellationToken);

        var relevantPosts = posts.Dados
            .Where(post => !string.IsNullOrWhiteSpace(post.Message))
            .OrderByDescending(post => post.CreatedAt)
            .Take(80)
            .ToList();

        if (relevantPosts.Count == 0)
        {
            return new AiInstructorFrequentQuestionsDto
            {
                Items = new List<AiFrequentQuestionItemDto>(),
                Model = _configuration.Model,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var forumDigest = new StringBuilder();
        foreach (var post in relevantPosts)
        {
            forumDigest.AppendLine($"Curso: {post.Thread?.Course?.Title ?? "Curso nao informado"}");
            forumDigest.AppendLine($"Turma: {post.Thread?.ClassGroup?.Name ?? "Turma nao informada"}");
            forumDigest.AppendLine($"Autor: {post.Author?.FullName ?? "Usuario"}");
            forumDigest.AppendLine($"Mensagem: {NormalizeWhitespace(StripHtml(post.Message))}");
            forumDigest.AppendLine();
        }

        var prompt = $$"""
        Voce e um assistente educacional em portugues do Brasil.
        Analise as mensagens de forum abaixo e identifique as duvidas mais frequentes dos alunos.
        Considere variacoes de redacao como a mesma duvida quando o tema for igual.
        Responda exclusivamente em JSON valido no formato:
        {
          "items": [
            {
              "topic": "tema curto",
              "question": "duvida representativa",
              "suggestedAction": "acao recomendada ao professor",
              "estimatedMentions": 3,
              "courseTitle": "nome do curso",
              "classGroupName": "nome da turma"
            }
          ]
        }
        Limite a resposta a no maximo 5 itens, ordenados do mais recorrente para o menos recorrente.

        Mensagens:
        {{forumDigest.ToString()}}
        """;

        var completion = await CreateCompletionAsync(prompt, cancellationToken);
        var parsed = DeserializeOrThrow<AiInstructorFrequentQuestionsPayload>(completion);

        return new AiInstructorFrequentQuestionsDto
        {
            Items = parsed.Items?
                .Where(item => !string.IsNullOrWhiteSpace(item.Question))
                .Take(5)
                .Select(item => new AiFrequentQuestionItemDto
                {
                    Topic = item.Topic?.Trim() ?? string.Empty,
                    Question = item.Question?.Trim() ?? string.Empty,
                    SuggestedAction = item.SuggestedAction?.Trim() ?? string.Empty,
                    EstimatedMentions = Math.Max(item.EstimatedMentions, 1),
                    CourseTitle = item.CourseTitle?.Trim() ?? string.Empty,
                    ClassGroupName = item.ClassGroupName?.Trim() ?? string.Empty
                })
                .ToList() ?? new List<AiFrequentQuestionItemDto>(),
            Model = _configuration.Model,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<string> CreateCompletionAsync(string prompt, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _configuration.Model,
            temperature = _configuration.Temperature,
            max_tokens = _configuration.MaxTokens,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = "Voce responde apenas com JSON valido." },
                new { role = "user", content = prompt }
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BusinessException($"Falha ao consultar o provedor de IA ({(int)response.StatusCode}).", ECodigo.NaoPermitido);
        }

        var completion = JsonSerializer.Deserialize<AiChatCompletionResponse>(body, JsonOptions);
        var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new BusinessException("O provedor de IA nao retornou conteudo utilizavel.", ECodigo.NaoPermitido);
        }

        return content;
    }

    private void EnsureConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
        {
            throw new BusinessException("A chave do provedor de IA nao esta configurada no servidor.", ECodigo.NaoPermitido);
        }

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(EnsureTrailingSlash(_configuration.BaseUrl));
        }
    }

    private static T DeserializeOrThrow<T>(string json)
    {
        var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
        if (result is null)
        {
            throw new BusinessException("Nao foi possivel interpretar a resposta do provedor de IA.", ECodigo.NaoPermitido);
        }

        return result;
    }

    private static string BuildContentSource(string? title, string? summary, string? body)
    {
        var source = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(title))
        {
            source.AppendLine(title);
        }

        if (!string.IsNullOrWhiteSpace(summary))
        {
            source.AppendLine(summary);
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            source.AppendLine(StripHtml(body));
        }

        return NormalizeWhitespace(source.ToString());
    }

    private static string StripHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var withoutTags = Regex.Replace(input, "<[^>]+>", " ");
        return System.Net.WebUtility.HtmlDecode(withoutTags);
    }

    private static string NormalizeWhitespace(string input)
    {
        return Regex.Replace(input ?? string.Empty, "\\s+", " ").Trim();
    }

    private static IList<string> SanitizeList(IEnumerable<string>? values, int maxItems)
    {
        return values?
            .Select(value => value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Take(maxItems)
            .Cast<string>()
            .ToList() ?? new List<string>();
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : $"{url}/";
    }

    private sealed class AiChatCompletionResponse
    {
        public IList<AiChatChoice>? Choices { get; set; }
    }

    private sealed class AiChatChoice
    {
        public AiChatMessage? Message { get; set; }
    }

    private sealed class AiChatMessage
    {
        public string? Content { get; set; }
    }

    private sealed class AiContentSummaryPayload
    {
        public string? Summary { get; set; }
        public IList<string>? KeyPoints { get; set; }
        public IList<string>? AttentionPoints { get; set; }
    }

    private sealed class AiInstructorFrequentQuestionsPayload
    {
        public IList<AiInstructorFrequentQuestionItemPayload>? Items { get; set; }
    }

    private sealed class AiInstructorFrequentQuestionItemPayload
    {
        public string? Topic { get; set; }
        public string? Question { get; set; }
        public string? SuggestedAction { get; set; }
        public int EstimatedMentions { get; set; }
        public string? CourseTitle { get; set; }
        public string? ClassGroupName { get; set; }
    }
}
