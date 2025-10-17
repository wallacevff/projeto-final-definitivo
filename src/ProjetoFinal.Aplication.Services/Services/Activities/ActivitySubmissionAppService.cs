using System;
using System.Collections.Generic;
using System.Linq;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Activities;

public class ActivitySubmissionAppService : IActivitySubmissionAppService
{
    private readonly IActivitySubmissionRepository _submissionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IVideoAnnotationRepository _videoAnnotationRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ActivitySubmissionAppService(
        IActivitySubmissionRepository submissionRepository,
        IActivityRepository activityRepository,
        IVideoAnnotationRepository videoAnnotationRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
    {
        _submissionRepository = submissionRepository;
        _activityRepository = activityRepository;
        _videoAnnotationRepository = videoAnnotationRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public async Task<ActivitySubmissionDto> SubmitAsync(ActivitySubmissionCreateDto dto, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.FindAsync(dto.ActivityId, cancellationToken);
        if (activity is null)
        {
            throw new BusinessException("Atividade nao encontrada.", ECodigo.NaoEncontrado);
        }

        if (activity.DueDate.HasValue && DateTime.UtcNow > activity.DueDate.Value && !activity.AllowLateSubmissions)
        {
            throw new BusinessException("Prazo para envio encerrado.", ECodigo.NaoPermitido);
        }

        var existingSubmission = await _submissionRepository.FirstOrDefaultByPredicateAsync(
            submission => submission.ActivityId == dto.ActivityId && submission.StudentId == dto.StudentId,
            cancellationToken);

        if (existingSubmission is not null)
        {
            throw new BusinessException("Voce ja enviou esta atividade.", ECodigo.Conflito);
        }

        var entity = new ActivitySubmission
        {
            ActivityId = dto.ActivityId,
            StudentId = dto.StudentId,
            ClassGroupId = dto.ClassGroupId,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            TextAnswer = dto.TextAnswer,
            Attachments = BuildAttachments(dto.Attachments)
        };

        var created = await _submissionRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        var withDetails = await _submissionRepository.GetWithDetailsAsync(created.Id, cancellationToken);
        return _mapper.MapFrom<ActivitySubmissionDto>(withDetails ?? created);
    }

    public async Task<ActivitySubmissionDto> UpdateAsync(Guid submissionId, ActivitySubmissionUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetWithDetailsAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            throw new BusinessException("Envio nao encontrado.", ECodigo.NaoEncontrado);
        }

        submission.Status = dto.Status;
        submission.Score = dto.Score;
        submission.GradedById = dto.GradedById;
        submission.Feedback = dto.Feedback;
        submission.TextAnswer = dto.TextAnswer;
        submission.GradedAt = dto.Score.HasValue ? DateTime.UtcNow : submission.GradedAt;
        submission.Attachments.Clear();
        foreach (var attachment in BuildAttachments(dto.Attachments))
        {
            submission.Attachments.Add(attachment);
        }
        submission.UpdatedAt = DateTime.UtcNow;

        await _submissionRepository.UpdateAsync(submission, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        var updated = await _submissionRepository.GetWithDetailsAsync(submission.Id, cancellationToken);
        return _mapper.MapFrom<ActivitySubmissionDto>(updated ?? submission);
    }

    public async Task<ActivitySubmissionDto> GetByIdAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetWithDetailsAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            throw new BusinessException("Envio nao encontrado.", ECodigo.NaoEncontrado);
        }

        return _mapper.MapFrom<ActivitySubmissionDto>(submission);
    }

    public async Task<PagedResultDto<ActivitySubmissionDto>> GetAllAsync(ActivitySubmissionFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await _submissionRepository.GetAllAsync(filter, cancellationToken);
        return _mapper.MapFrom<PagedResultDto<ActivitySubmissionDto>>(result);
    }

    public async Task<VideoAnnotationDto> AddAnnotationAsync(VideoAnnotationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetWithDetailsAsync(dto.SubmissionId, cancellationToken);
        if (submission is null)
        {
            throw new BusinessException("Envio nao encontrado.", ECodigo.NaoEncontrado);
        }

        var attachment = submission.Attachments.FirstOrDefault(a => a.Id == dto.AttachmentId);
        if (attachment is null || !attachment.IsVideo)
        {
            throw new BusinessException("Somente videos permitem anotacoes temporais.", ECodigo.MaRequisicao);
        }

        var annotation = new VideoAnnotation
        {
            SubmissionId = dto.SubmissionId,
            AttachmentId = dto.AttachmentId,
            CreatedById = dto.CreatedById,
            TimeMarkerSeconds = dto.TimeMarkerSeconds,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _videoAnnotationRepository.AddAsync(annotation, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<VideoAnnotationDto>(created);
    }

    public async Task<VideoAnnotationDto> UpdateAnnotationAsync(Guid annotationId, VideoAnnotationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var annotation = await _videoAnnotationRepository.FindAsync(annotationId, cancellationToken);
        if (annotation is null)
        {
            throw new BusinessException("Anotacao nao encontrada.", ECodigo.NaoEncontrado);
        }

        annotation.TimeMarkerSeconds = dto.TimeMarkerSeconds;
        annotation.Comment = dto.Comment;
        annotation.EditedAt = DateTime.UtcNow;

        await _videoAnnotationRepository.UpdateAsync(annotation, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<VideoAnnotationDto>(annotation);
    }

    public async Task DeleteAnnotationAsync(Guid annotationId, CancellationToken cancellationToken = default)
    {
        var annotation = await _videoAnnotationRepository.FindAsync(annotationId, cancellationToken);
        if (annotation is null)
        {
            throw new BusinessException("Anotacao nao encontrada.", ECodigo.NaoEncontrado);
        }

        await _videoAnnotationRepository.DeleteAsync(annotation, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<SubmissionAttachment> BuildAttachments(IList<SubmissionAttachmentCreateDto> attachments)
    {
        attachments ??= new List<SubmissionAttachmentCreateDto>();
        var result = new List<SubmissionAttachment>();
        foreach (var attachmentDto in attachments)
        {
            result.Add(new SubmissionAttachment
            {
                MediaResourceId = attachmentDto.MediaResourceId,
                IsPrimary = attachmentDto.IsPrimary,
                IsVideo = attachmentDto.IsVideo
            });
        }

        return result;
    }
}
