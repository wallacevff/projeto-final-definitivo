using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoFinal.Api.Controllers;

[Route("api/class-groups")]
[Route("api/v1/class-groups")]
public class ClassGroupsController : BaseController<
    ClassGroupDto,
    ClassGroupCreateDto,
    ClassGroupUpdateDto,
    ClassGroupFilter,
    Guid,
    IClassGroupAppService>
{
    private readonly ICourseAppService _courseService;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IClassEnrollmentRepository _classEnrollmentRepository;

    public ClassGroupsController(
        IClassGroupAppService service,
        ICourseAppService courseService,
        IClassGroupRepository classGroupRepository,
        IClassEnrollmentRepository classEnrollmentRepository) : base(service)
    {
        _courseService = courseService;
        _classGroupRepository = classGroupRepository;
        _classEnrollmentRepository = classEnrollmentRepository;
    }

    [HttpGet]
    public override async Task<PagedResultDto<ClassGroupDto>> GetAllAsync(
        [FromQuery] ClassGroupFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return await base.GetAllAsync(filter, cancellationToken);
        }

        if (filter.CourseId is null || filter.CourseId == Guid.Empty)
        {
            return new PagedResultDto<ClassGroupDto>
            {
                Dados = new List<ClassGroupDto>(),
                PageInfo = new PageInfoDto
                {
                    PageNumber = filter.PageNumber == 0 ? 1 : filter.PageNumber,
                    PageSize = filter.PageSize == 0 ? 0 : filter.PageSize,
                    TotalPages = 0,
                    TotalItens = 0
                }
            };
        }

        await EnsureInstructorOwnsCourseAsync(filter.CourseId.Value, cancellationToken);
        return await base.GetAllAsync(filter, cancellationToken);
    }

    [HttpGet("{id}")]
    public override async Task<ClassGroupDto> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsClassGroupAsync(id, cancellationToken);
        return await base.GetByIdAsync(id, cancellationToken);
    }

    [HttpPost]
    public override async Task<ClassGroupDto> AddAsync(
        [FromBody] ClassGroupCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsCourseAsync(createDto.CourseId, cancellationToken);
        return await base.AddAsync(createDto, cancellationToken);
    }

    [HttpPut("{id}")]
    public override async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] ClassGroupUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsClassGroupAsync(id, cancellationToken);
        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    [HttpDelete("{id}")]
    public override async Task<ClassGroupDto> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsClassGroupAsync(id, cancellationToken);
        return await base.DeleteAsync(id, cancellationToken);
    }

    [HttpPost("{classGroupId:guid}/enrollments")]
    public async Task<ActionResult<ClassEnrollmentDto>> RequestEnrollmentAsync(
        [FromRoute] Guid classGroupId,
        [FromBody] ClassEnrollmentRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
        {
            return BadRequest("Dados de inscricao nao informados.");
        }

        if (dto.ClassGroupId == Guid.Empty)
        {
            dto.ClassGroupId = classGroupId;
        }
        else if (dto.ClassGroupId != classGroupId)
        {
            return BadRequest("ClassGroupId divergente do valor na rota.");
        }

        var studentId = ResolveCurrentUserId();
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("Aluno nao identificado.", ECodigo.NaoAutenticado);
        }

        dto.StudentId = studentId;

        var enrollment = await Service.RequestEnrollmentAsync(dto, cancellationToken);
        return Ok(enrollment);
    }

    [HttpPost("enrollments/decision")]
    public async Task<ClassEnrollmentDto> DecideEnrollmentAsync(
        [FromBody] ClassEnrollmentDecisionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsEnrollmentAsync(dto.EnrollmentId, cancellationToken);
        return await Service.DecideEnrollmentAsync(dto, cancellationToken);
    }

    [HttpDelete("enrollments/{enrollmentId:guid}")]
    public async Task<IActionResult> RemoveEnrollmentAsync(
        [FromRoute] Guid enrollmentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsEnrollmentAsync(enrollmentId, cancellationToken);
        await Service.RemoveEnrollmentAsync(enrollmentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{classGroupId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> HasAvailableSeatsAsync(
        [FromRoute] Guid classGroupId,
        CancellationToken cancellationToken = default)
    {
        var hasSeats = await Service.HasAvailableSeatsAsync(classGroupId, cancellationToken);
        return Ok(new AvailabilityResponse(hasSeats));
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private bool IsInstructor()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Instructor), StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAdministrator()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Administrator), StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureInstructorOwnsCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        var course = await _courseService.GetByIdAsync(courseId, cancellationToken);
        if (course.InstructorId != instructorId)
        {
            throw new BusinessException("Acesso nao permitido a turma.", ECodigo.NaoPermitido);
        }
    }

    private async Task EnsureInstructorOwnsClassGroupAsync(Guid classGroupId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var group = await _classGroupRepository.FindAsync(classGroupId, cancellationToken);
        if (group is null)
        {
            throw new BusinessException("Turma nao encontrada.", ECodigo.NaoEncontrado);
        }

        await EnsureInstructorOwnsCourseAsync(group.CourseId, cancellationToken);
    }

    private async Task EnsureInstructorOwnsEnrollmentAsync(Guid enrollmentId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var enrollment = await _classEnrollmentRepository.FindAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            throw new BusinessException("Inscricao nao encontrada.", ECodigo.NaoEncontrado);
        }

        await EnsureInstructorOwnsClassGroupAsync(enrollment.ClassGroupId, cancellationToken);
    }

    public record AvailabilityResponse(bool HasSeats);
}
