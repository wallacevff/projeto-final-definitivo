namespace ProjetoFinal.Application.Contracts.Dto.Ai;

public class AiFrequentQuestionItemDto
{
    public string Topic { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
    public int EstimatedMentions { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string ClassGroupName { get; set; } = string.Empty;
}
