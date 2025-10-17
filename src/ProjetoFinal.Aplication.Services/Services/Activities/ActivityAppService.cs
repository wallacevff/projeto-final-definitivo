using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Activities;

public class ActivityAppService : DefaultService<Activity, ActivityDto, ActivityCreateDto, ActivityUpdateDto, ActivityFilter, Guid>, IActivityAppService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ActivityAppService(
        IActivityRepository activityRepository,
        IClassGroupRepository classGroupRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(activityRepository, unityOfWork, mapper)
    {
        _activityRepository = activityRepository;
        _classGroupRepository = classGroupRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<ActivityDto> AddAsync(ActivityCreateDto dto, CancellationToken cancellationToken = default)
    {
        var classGroup = await _classGroupRepository.FindAsync(dto.ClassGroupId, cancellationToken);
        if (classGroup is null)
        {
            throw new BusinessException("Turma nao encontrada para criar a atividade.", ECodigo.NaoEncontrado);
        }

        if (dto.CourseId != Guid.Empty && dto.CourseId != classGroup.CourseId)
        {
            throw new BusinessException("Turma informada nao pertence ao curso selecionado.", ECodigo.Conflito);
        }

        var entity = _mapper.MapFrom<Activity>(dto);
        entity.Attachments = _mapper.MapFrom<List<ActivityAttachment>>(dto.Attachments);
        entity.ClassGroupId = classGroup.Id;
        entity.CourseId = classGroup.CourseId;
        entity.Scope = ActivityScope.ClassGroup;
        entity.Audiences = BuildAudiences(classGroup.Id, entity.Id);
        if (!dto.AllowLateSubmissions)
        {
            entity.LatePenaltyPercentage = null;
        }

        var created = await _activityRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ActivityDto>(created);
    }

    public override async Task UpdateAsync(ActivityUpdateDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.FindAsync(id, cancellationToken);
        if (activity is null)
        {
            throw new BusinessException("Atividade nao encontrada.", ECodigo.NaoEncontrado);
        }

        var classGroup = await _classGroupRepository.FindAsync(dto.ClassGroupId, cancellationToken);
        if (classGroup is null)
        {
            throw new BusinessException("Turma informada na atividade nao existe.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, activity);
        activity.ClassGroupId = classGroup.Id;
        activity.CourseId = classGroup.CourseId;
        activity.Scope = ActivityScope.ClassGroup;
        activity.Attachments.Clear();
        foreach (var attachmentDto in dto.Attachments)
        {
            var attachment = _mapper.MapFrom<ActivityAttachment>(attachmentDto);
            attachment.Id = Guid.NewGuid();
            attachment.ActivityId = activity.Id;
            activity.Attachments.Add(attachment);
        }

        activity.Audiences.Clear();
        foreach (var audience in BuildAudiences(classGroup.Id, activity.Id))
        {
            activity.Audiences.Add(audience);
        }
        if (!dto.AllowLateSubmissions)
        {
            activity.LatePenaltyPercentage = null;
        }

        activity.UpdatedAt = DateTime.UtcNow;
        await _activityRepository.UpdateAsync(activity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<ActivityAudience> BuildAudiences(Guid classGroupId, Guid activityId)
    {
        return new List<ActivityAudience>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                ClassGroupId = classGroupId
            }
        };
    }
}
