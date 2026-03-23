using System;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;
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
            .Include(message => message.Recipient)
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

    protected override Expression<Func<ChatMessage, bool>> GetFilters(ChatMessageFilter filter)
    {
        var predicate = base.GetFilters(filter);

        if (filter.ClassGroupId is null || filter.ClassGroupId == Guid.Empty)
        {
            return predicate;
        }

        if (filter.RecipientId is null || filter.RecipientId == Guid.Empty)
        {
            predicate = predicate.And(message => message.RecipientId == null);
            return predicate;
        }

        if (filter.CurrentUserId is null || filter.CurrentUserId == Guid.Empty)
        {
            predicate = predicate.And(message => message.RecipientId == filter.RecipientId);
            return predicate;
        }

        var currentUserId = filter.CurrentUserId.Value;
        var participantId = filter.RecipientId.Value;
        predicate = predicate.And(message =>
            (message.SenderId == currentUserId && message.RecipientId == participantId)
            || (message.SenderId == participantId && message.RecipientId == currentUserId));

        return predicate;
    }
}
