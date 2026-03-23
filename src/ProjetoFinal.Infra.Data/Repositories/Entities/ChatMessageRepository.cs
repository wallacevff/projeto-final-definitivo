using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ChatMessageRepository(AppDbContext context)
    : DefaultRepository<ChatMessage, ChatMessageFilter, Guid>(context), IChatMessageRepository
{
    protected override IQueryable<ChatMessage> ApplyIncludes(IQueryable<ChatMessage> query)
    {
        return query
            .Include(message => message.Sender)
            .Include(message => message.MediaResource);
    }

    protected override IQueryable<ChatMessage> ApplyIncludesList(IQueryable<ChatMessage> query)
    {
        return ApplyIncludes(query);
    }

    protected override IQueryable<ChatMessage> ApplyOrderBy(IQueryable<ChatMessage> query)
    {
        return query
            .OrderBy(message => message.SentAt)
            .ThenBy(message => message.Id);
    }
}
