using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ContentVideoAnnotationRepository
    : DefaultRepository<ContentVideoAnnotation, ContentVideoAnnotationFilter, Guid>,
        IContentVideoAnnotationRepository
{
    public ContentVideoAnnotationRepository(AppDbContext context) : base(context)
    {
    }
}
