using System;

namespace ProjetoFinal.Application.Contracts.Dto.Contents;

public class ContentVideoAnnotationDto
{
    public Guid Id { get; set; }
    public Guid ContentAttachmentId { get; set; }
    public Guid CreatedById { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
}

public class ContentVideoAnnotationCreateDto
{
    public Guid ContentAttachmentId { get; set; }
    public Guid CreatedById { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ContentVideoAnnotationUpdateDto
{
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
}
