using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/course-subscriptions")]
public class CourseSubscriptionsController : BaseController<
    CourseSubscriptionDto,
    CourseSubscriptionCreateDto,
    CourseSubscriptionCreateDto,
    CourseSubscriptionFilter,
    Guid,
    ICourseSubscriptionAppService>
{
    public CourseSubscriptionsController(ICourseSubscriptionAppService service) : base(service)
    {
    }
}
