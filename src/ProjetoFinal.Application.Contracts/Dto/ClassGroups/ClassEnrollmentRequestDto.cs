using System;

namespace ProjetoFinal.Application.Contracts.Dto.ClassGroups;

public class ClassEnrollmentRequestDto
{
    public Guid ClassGroupId { get; set; }
    public Guid StudentId { get; set; }
    public string? EnrollmentCode { get; set; }
}
