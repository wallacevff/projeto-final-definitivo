namespace ProjetoFinal.Application.Contracts.Dto.Ai;

public class AiInstructorFrequentQuestionsDto
{
    public IList<AiFrequentQuestionItemDto> Items { get; set; } = new List<AiFrequentQuestionItemDto>();
    public string Model { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
