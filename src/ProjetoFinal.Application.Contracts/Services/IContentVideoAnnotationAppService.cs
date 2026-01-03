using System;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IContentVideoAnnotationAppService
    : IDefaultService<ContentVideoAnnotationDto, ContentVideoAnnotationCreateDto, ContentVideoAnnotationUpdateDto,
        ContentVideoAnnotationFilter, Guid>
{
}
