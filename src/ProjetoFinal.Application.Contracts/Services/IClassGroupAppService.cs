using System;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IClassGroupAppService : IDefaultService<ClassGroupDto, ClassGroupCreateDto, ClassGroupUpdateDto, ClassGroupFilter, Guid>
{
    Task<ClassEnrollmentDto> RequestEnrollmentAsync(ClassEnrollmentRequestDto dto, CancellationToken cancellationToken = default);
    Task<ClassEnrollmentDto> DecideEnrollmentAsync(ClassEnrollmentDecisionDto dto, CancellationToken cancellationToken = default);
    Task RemoveEnrollmentAsync(Guid enrollmentId, CancellationToken cancellationToken = default);
    Task<bool> HasAvailableSeatsAsync(Guid classGroupId, CancellationToken cancellationToken = default);
}
