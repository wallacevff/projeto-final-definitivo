using System.ComponentModel.DataAnnotations;

namespace ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

public class MinioConfiguration
{
    public const string SectionName = "Minio";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;

    [Required]
    public string BucketName { get; set; } = string.Empty;
}
