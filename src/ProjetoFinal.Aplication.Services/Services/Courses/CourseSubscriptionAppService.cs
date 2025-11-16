using System;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Courses;

public class CourseSubscriptionAppService : DefaultService<CourseSubscription, CourseSubscriptionDto, CourseSubscriptionCreateDto, CourseSubscriptionCreateDto, CourseSubscriptionFilter, Guid>, ICourseSubscriptionAppService
{
    private readonly ICourseSubscriptionRepository _subscriptionRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnityOfWork _unityOfWork;
    private readonly IAutomapApi _mapper;

    public CourseSubscriptionAppService(
        ICourseSubscriptionRepository repository,
        ICourseRepository courseRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(repository, unityOfWork, mapper)
    {
        _subscriptionRepository = repository;
        _courseRepository = courseRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<CourseSubscriptionDto> AddAsync(CourseSubscriptionCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CourseId == Guid.Empty)
        {
            throw new BusinessException("Curso obrigatorio para inscricao.", ECodigo.MaRequisicao);
        }

        if (dto.StudentId == Guid.Empty)
        {
            throw new BusinessException("Aluno nao identificado para realizar a inscricao.", ECodigo.NaoAutenticado);
        }

        var course = await _courseRepository.FindAsync(dto.CourseId, cancellationToken);
        if (course is null)
        {
            throw new BusinessException("Curso nao encontrado.", ECodigo.NaoEncontrado);
        }

        if (course.Mode != CourseMode.MaterialsDistribution)
        {
            throw new BusinessException("A inscricao direta so esta disponivel para cursos de distribuicao de materiais.", ECodigo.NaoPermitido);
        }

        if (!course.IsPublished)
        {
            throw new BusinessException("Este curso ainda nao foi publicado.", ECodigo.Conflito);
        }

        var existing = await _subscriptionRepository.FirstOrDefaultByPredicateAsync(
            subscription => subscription.CourseId == dto.CourseId && subscription.StudentId == dto.StudentId,
            cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException("Voce ja esta inscrito neste curso.", ECodigo.Conflito);
        }

        var entity = _mapper.MapFrom<CourseSubscription>(dto);
        entity.SubscribedAt = DateTime.UtcNow;

        var created = await _subscriptionRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<CourseSubscriptionDto>(created);
    }
}
