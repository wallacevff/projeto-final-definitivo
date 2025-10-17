using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.ClassGroups;

public class ClassEnrollmentDecisionDto
{
    public Guid EnrollmentId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public Guid DecidedById { get; set; }
    public string? DecisionReason { get; set; }
}
