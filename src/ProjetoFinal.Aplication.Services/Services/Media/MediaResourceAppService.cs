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
    public MediaResourceAppService(
        IMediaResourceRepository repository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(repository, unityOfWork, mapper)
    {
    }
}
