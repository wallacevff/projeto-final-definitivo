using System;
using System.Collections.Generic;

namespace ProjetoFinal.Domain.Entities;

public class CourseCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; } = true;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
