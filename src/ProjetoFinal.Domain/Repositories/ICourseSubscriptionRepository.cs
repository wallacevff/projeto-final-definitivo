using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface ICourseSubscriptionRepository : IDefaultRepository<CourseSubscription, CourseSubscriptionFilter, Guid>
{
}
