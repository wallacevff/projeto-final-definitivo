using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Filters;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ContentAttachmentRepository(AppDbContext context)
    : DefaultRepository<ContentAttachment, Filter, Guid>(context), IContentAttachmentRepository
{
}
