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
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ActivityAppService(
        IActivityRepository activityRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(activityRepository, unityOfWork, mapper)
    {
        _activityRepository = activityRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<ActivityDto> AddAsync(ActivityCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.MapFrom<Activity>(dto);
        entity.Attachments = _mapper.MapFrom<List<ActivityAttachment>>(dto.Attachments);
        entity.Audiences = BuildAudiences(entity.Scope, dto.ClassGroupIds, entity.Id);
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

        _mapper.MapTo(dto, activity);
        activity.Attachments.Clear();
        foreach (var attachmentDto in dto.Attachments)
        {
            var attachment = _mapper.MapFrom<ActivityAttachment>(attachmentDto);
            attachment.Id = Guid.NewGuid();
            attachment.ActivityId = activity.Id;
            activity.Attachments.Add(attachment);
        }

        activity.Audiences = BuildAudiences(activity.Scope, dto.ClassGroupIds, activity.Id);
        if (!dto.AllowLateSubmissions)
        {
            activity.LatePenaltyPercentage = null;
        }

        activity.UpdatedAt = DateTime.UtcNow;
        await _activityRepository.UpdateAsync(activity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<ActivityAudience> BuildAudiences(ActivityScope scope, IList<Guid> classGroupIds, Guid activityId)
    {
        if (scope == ActivityScope.Course)
        {
            return new List<ActivityAudience>();
        }

        var audiences = new List<ActivityAudience>();
        foreach (var classGroupId in classGroupIds.Distinct())
        {
            audiences.Add(new ActivityAudience
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                ClassGroupId = classGroupId
            });
        }
        return audiences;
    }
}
