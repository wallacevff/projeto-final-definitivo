using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IMediaResourceRepository : IDefaultRepository<MediaResource, MediaResourceFilter, Guid>
{
}
