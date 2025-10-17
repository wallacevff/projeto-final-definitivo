using System;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class VideoAnnotationUpdateDto
{
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
}
