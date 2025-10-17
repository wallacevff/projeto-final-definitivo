using System.ComponentModel.DataAnnotations;

namespace ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

public class JwtConfiguration
{
    public const string SectionName = "Jwt";

    [Required]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ExpiresInMinutes { get; set; } = 60;
}
