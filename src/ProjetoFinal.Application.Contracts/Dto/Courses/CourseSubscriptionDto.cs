using System;

namespace ProjetoFinal.Application.Contracts.Dto.Courses;

public class CourseSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime SubscribedAt { get; set; }
}

public class CourseSubscriptionCreateDto
{
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
}
