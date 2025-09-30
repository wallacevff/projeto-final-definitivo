using System;

namespace ProjetoFinal.Application.Contracts.Dto.Courses;

public class CourseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
}
