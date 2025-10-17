using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.ClassGroups;

public class ClassGroupDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresEnrollmentCode { get; set; }
    public bool EnableChat { get; set; }
    public bool IsMaterialsDistribution { get; set; }
    public DateTime? EnrollmentOpensAt { get; set; }
    public DateTime? EnrollmentClosesAt { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int ApprovedEnrollments { get; set; }
    public int PendingEnrollments { get; set; }

    public IList<ClassEnrollmentDto> Enrollments { get; set; } = new List<ClassEnrollmentDto>();
}

public class ClassEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid ClassGroupId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public EnrollmentStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? DecisionAt { get; set; }
    public Guid? DecidedById { get; set; }
}
