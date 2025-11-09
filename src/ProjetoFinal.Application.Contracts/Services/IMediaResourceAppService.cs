using System;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IMediaResourceAppService : IDefaultService<MediaResourceDto, MediaResourceCreateDto, MediaResourceCreateDto, MediaResourceFilter, Guid>
{
    Task<MediaResourceDto?> FindByShaAsync(string sha256, CancellationToken cancellationToken = default);
}
