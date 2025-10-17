using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IForumPostRepository : IDefaultRepository<ForumPost, ForumPostFilter, Guid>
{
}
