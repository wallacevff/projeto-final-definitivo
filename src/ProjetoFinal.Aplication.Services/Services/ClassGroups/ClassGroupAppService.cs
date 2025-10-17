using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.ClassGroups;

public class ClassGroupAppService : DefaultService<ClassGroup, ClassGroupDto, ClassGroupCreateDto, ClassGroupUpdateDto, ClassGroupFilter, Guid>, IClassGroupAppService
{
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IClassEnrollmentRepository _classEnrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ClassGroupAppService(
        IClassGroupRepository classGroupRepository,
        IClassEnrollmentRepository classEnrollmentRepository,
        ICourseRepository courseRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(classGroupRepository, unityOfWork, mapper)
    {
        _classGroupRepository = classGroupRepository;
        _classEnrollmentRepository = classEnrollmentRepository;
        _courseRepository = courseRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<ClassGroupDto> AddAsync(ClassGroupCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.RequiresEnrollmentCode && string.IsNullOrWhiteSpace(dto.EnrollmentCode))
        {
            throw new BusinessException("Informe um codigo de inscricao para a turma.", ECodigo.MaRequisicao);
        }

        var entity = _mapper.MapFrom<ClassGroup>(dto);
        entity.EnableChat = dto.EnableChat;
        entity.RequiresApproval = dto.RequiresApproval;
        entity.RequiresEnrollmentCode = dto.RequiresEnrollmentCode;
        entity.EnrollmentCodeHash = dto.RequiresEnrollmentCode ? HashEnrollmentCode(dto.EnrollmentCode!) : null;

        var course = await _courseRepository.FindAsync(dto.CourseId, cancellationToken);
        if (course is null)
        {
            throw new BusinessException("Curso associado a turma nao encontrado.", ECodigo.NaoEncontrado);
        }

        if (course.Mode == CourseMode.MaterialsDistribution)
        {
            var existingGroup = await _classGroupRepository.FirstOrDefaultByPredicateAsync(
                group => group.CourseId == dto.CourseId,
                cancellationToken);
            if (existingGroup is not null)
            {
                throw new BusinessException("Cursos de distribuicao de material suportam apenas uma turma.", ECodigo.Conflito);
            }

            entity.IsMaterialsDistribution = true;
        }
        else
        {
            entity.IsMaterialsDistribution = dto.IsMaterialsDistribution;
            if (entity.IsMaterialsDistribution)
            {
                var flaggedGroup = await _classGroupRepository.FirstOrDefaultByPredicateAsync(
                    group => group.CourseId == dto.CourseId && group.IsMaterialsDistribution,
                    cancellationToken);
                if (flaggedGroup is not null)
                {
                    throw new BusinessException("Este curso ja possui uma turma marcada como distribuicao de material.", ECodigo.Conflito);
                }
            }
        }

        var created = await _classGroupRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ClassGroupDto>(created);
    }

    public override async Task UpdateAsync(ClassGroupUpdateDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _classGroupRepository.FindAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new BusinessException("Turma nao encontrada.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, entity);
        entity.EnableChat = dto.EnableChat;
        entity.RequiresApproval = dto.RequiresApproval;
        entity.RequiresEnrollmentCode = dto.RequiresEnrollmentCode;
        if (dto.RequiresEnrollmentCode)
        {
            if (!string.IsNullOrWhiteSpace(dto.EnrollmentCode))
            {
                entity.EnrollmentCodeHash = HashEnrollmentCode(dto.EnrollmentCode);
            }
        }
        else
        {
            entity.EnrollmentCodeHash = null;
        }

        var course = await _courseRepository.FindAsync(entity.CourseId, cancellationToken);
        if (course is null)
        {
            throw new BusinessException("Curso associado a turma nao encontrado.", ECodigo.NaoEncontrado);
        }

        if (course.Mode == CourseMode.MaterialsDistribution)
        {
            entity.IsMaterialsDistribution = true;
        }
        else
        {
            if (dto.IsMaterialsDistribution)
            {
                var flaggedGroup = await _classGroupRepository.FirstOrDefaultByPredicateAsync(
                    group => group.CourseId == entity.CourseId && group.IsMaterialsDistribution && group.Id != entity.Id,
                    cancellationToken);
                if (flaggedGroup is not null)
                {
                    throw new BusinessException("Este curso ja possui outra turma marcada como distribuicao de material.", ECodigo.Conflito);
                }
            }

            entity.IsMaterialsDistribution = dto.IsMaterialsDistribution;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _classGroupRepository.UpdateAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ClassEnrollmentDto> RequestEnrollmentAsync(ClassEnrollmentRequestDto dto, CancellationToken cancellationToken = default)
    {
        var classGroup = await _classGroupRepository.FindAsync(dto.ClassGroupId, cancellationToken);
        if (classGroup is null)
        {
            throw new BusinessException("Turma nao encontrada.", ECodigo.NaoEncontrado);
        }

        var alreadyRequested = await _classEnrollmentRepository.FirstOrDefaultByPredicateAsync(
            enrollment => enrollment.ClassGroupId == dto.ClassGroupId && enrollment.StudentId == dto.StudentId,
            cancellationToken);

        if (alreadyRequested is not null)
        {
            throw new BusinessException("Voce ja possui uma solicitacao para esta turma.", ECodigo.Conflito);
        }

        if (classGroup.RequiresEnrollmentCode)
        {
            if (string.IsNullOrWhiteSpace(dto.EnrollmentCode))
            {
                throw new BusinessException("Codigo de inscricao obrigatorio para esta turma.", ECodigo.MaRequisicao);
            }

            var hashedCode = HashEnrollmentCode(dto.EnrollmentCode);
            if (!string.Equals(hashedCode, classGroup.EnrollmentCodeHash, StringComparison.Ordinal))
            {
                throw new BusinessException("Codigo de inscricao invalido.", ECodigo.NaoPermitido);
            }
        }

        var hasSeats = await _classGroupRepository.HasAvailableSeatsAsync(classGroup.Id, cancellationToken);
        if (!classGroup.RequiresApproval && !hasSeats)
        {
            throw new BusinessException("Nao ha vagas disponiveis na turma.", ECodigo.Conflito);
        }

        var enrollment = new ClassEnrollment
        {
            ClassGroupId = dto.ClassGroupId,
            StudentId = dto.StudentId,
            Status = classGroup.RequiresApproval ? EnrollmentStatus.Pending : EnrollmentStatus.Approved,
            RequestedAt = DateTime.UtcNow,
            DecisionAt = classGroup.RequiresApproval ? null : DateTime.UtcNow
        };

        var created = await _classEnrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ClassEnrollmentDto>(created);
    }

    public async Task<ClassEnrollmentDto> DecideEnrollmentAsync(ClassEnrollmentDecisionDto dto, CancellationToken cancellationToken = default)
    {
        var enrollment = await _classEnrollmentRepository.FindAsync(dto.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            throw new BusinessException("Inscricao nao encontrada.", ECodigo.NaoEncontrado);
        }

        if (enrollment.Status != EnrollmentStatus.Pending)
        {
            throw new BusinessException("Esta inscricao ja foi analisada.", ECodigo.Conflito);
        }

        if (dto.Status == EnrollmentStatus.Approved)
        {
            var hasSeats = await _classGroupRepository.HasAvailableSeatsAsync(enrollment.ClassGroupId, cancellationToken);
            if (!hasSeats)
            {
                throw new BusinessException("Nao ha vagas disponiveis na turma.", ECodigo.Conflito);
            }
        }

        enrollment.Status = dto.Status;
        enrollment.DecisionAt = DateTime.UtcNow;
        enrollment.DecisionReason = dto.DecisionReason;
        enrollment.DecidedById = dto.DecidedById;

        await _classEnrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ClassEnrollmentDto>(enrollment);
    }

    public async Task RemoveEnrollmentAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _classEnrollmentRepository.FindAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            throw new BusinessException("Inscricao nao encontrada.", ECodigo.NaoEncontrado);
        }

        await _classEnrollmentRepository.DeleteAsync(enrollment, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasAvailableSeatsAsync(Guid classGroupId, CancellationToken cancellationToken = default)
    {
        return _classGroupRepository.HasAvailableSeatsAsync(classGroupId, cancellationToken);
    }

    private static string HashEnrollmentCode(string enrollmentCode)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(enrollmentCode.Trim());
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
