using System;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Contents;

public class ContentVideoAnnotationAppService
    : DefaultService<ContentVideoAnnotation, ContentVideoAnnotationDto, ContentVideoAnnotationCreateDto,
        ContentVideoAnnotationUpdateDto, ContentVideoAnnotationFilter, Guid>,
        IContentVideoAnnotationAppService
{
    public ContentVideoAnnotationAppService(
        IContentVideoAnnotationRepository repository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(repository, unityOfWork, mapper)
    {
    }
}
