using System;

namespace ProjetoFinal.Application.Contracts.Dto.ClassGroups;

public class ClassGroupUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresEnrollmentCode { get; set; }
    public string? EnrollmentCode { get; set; }
    public bool EnableChat { get; set; }
    public DateTime? EnrollmentOpensAt { get; set; }
    public DateTime? EnrollmentClosesAt { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
