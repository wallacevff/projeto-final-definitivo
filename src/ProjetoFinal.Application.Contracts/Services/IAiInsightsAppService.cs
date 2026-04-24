using ProjetoFinal.Application.Contracts.Dto.Ai;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IAiInsightsAppService
{
    Task<AiContentSummaryDto> GenerateContentSummaryAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<AiInstructorFrequentQuestionsDto> GetInstructorFrequentQuestionsAsync(Guid instructorId, CancellationToken cancellationToken = default);
}
