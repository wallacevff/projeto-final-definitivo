using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Courses;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? DetailedDescription { get; set; }
    public CourseMode Mode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public Guid? ThumbnailMediaId { get; set; }
    public bool EnableForum { get; set; }
    public bool EnableChat { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? EnrollmentInstructions { get; set; }

    public IList<ClassGroupDto> ClassGroups { get; set; } = new List<ClassGroupDto>();
}
