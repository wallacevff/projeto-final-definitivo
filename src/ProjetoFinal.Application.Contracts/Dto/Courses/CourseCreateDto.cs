using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Courses;

public class CourseCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? DetailedDescription { get; set; }
    public CourseMode Mode { get; set; }
    public Guid CategoryId { get; set; }
    public Guid InstructorId { get; set; }
    public Guid? ThumbnailMediaId { get; set; }
    public bool EnableForum { get; set; } = true;
    public bool EnableChat { get; set; }
    public string? EnrollmentInstructions { get; set; }
}
