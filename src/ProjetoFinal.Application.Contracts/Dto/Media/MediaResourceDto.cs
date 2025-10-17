using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Media;

public class MediaResourceDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public MediaKind Kind { get; set; }
    public long SizeInBytes { get; set; }
    public double? DurationInSeconds { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Sha256 { get; set; }
}
