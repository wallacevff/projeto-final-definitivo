using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IContentVideoAnnotationRepository
    : IDefaultRepository<ContentVideoAnnotation, ContentVideoAnnotationFilter, Guid>
{
}
