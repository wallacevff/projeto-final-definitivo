namespace ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

public class AiProviderConfiguration
{
    public const string SectionName = "AiProvider";

    public string BaseUrl { get; set; } = "https://api.deepseek.com";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "deepseek-chat";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 900;
}
