using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IContentAttachmentRepository : IDefaultRepository<ContentAttachment, Filter, Guid>
{
}
