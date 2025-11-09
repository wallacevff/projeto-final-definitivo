using System;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Media;

public class MediaResourceAppService : DefaultService<MediaResource, MediaResourceDto, MediaResourceCreateDto, MediaResourceCreateDto, MediaResourceFilter, Guid>, IMediaResourceAppService
{
    private readonly IMediaResourceRepository _repository;
    private readonly IAutomapApi _mapper;

    public MediaResourceAppService(
        IMediaResourceRepository repository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(repository, unityOfWork, mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MediaResourceDto?> FindByShaAsync(string sha256, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sha256))
        {
            return null;
        }

        var media = await _repository.GetByShaAsync(sha256, cancellationToken);
        return media is null ? null : _mapper.MapFrom<MediaResourceDto>(media);
    }
}
