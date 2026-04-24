namespace ProjetoFinal.Application.Contracts.Dto.Ai;

public class AiContentSummaryDto
{
    public string Summary { get; set; } = string.Empty;
    public IList<string> KeyPoints { get; set; } = new List<string>();
    public IList<string> AttentionPoints { get; set; } = new List<string>();
    public string Model { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
