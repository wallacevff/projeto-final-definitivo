namespace ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

public class UploadsConfiguration
{
    public const string ConfigurationSection = "UploadsConfiguration";
    public string PathDir { get; set; } = string.Empty;
}