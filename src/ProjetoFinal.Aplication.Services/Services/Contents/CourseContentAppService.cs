using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Contents;

public class CourseContentAppService : DefaultService<CourseContent, CourseContentDto, CourseContentCreateDto, CourseContentUpdateDto, CourseContentFilter, Guid>, ICourseContentAppService
{
    private readonly ICourseContentRepository _contentRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public CourseContentAppService(
        ICourseContentRepository contentRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(contentRepository, unityOfWork, mapper)
    {
        _contentRepository = contentRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<CourseContentDto> AddAsync(CourseContentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.MapFrom<CourseContent>(dto);
        entity.Attachments = _mapper.MapFrom<List<ContentAttachment>>(dto.Attachments);
        entity.PublishedAt = dto.IsDraft ? null : DateTime.UtcNow;

        var created = await _contentRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<CourseContentDto>(created);
    }

    public override async Task UpdateAsync(CourseContentUpdateDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.FindAsync(id, cancellationToken);
        if (content is null)
        {
            throw new BusinessException("Conteudo nao encontrado.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, content);
        content.Attachments.Clear();
        foreach (var attachmentDto in dto.Attachments)
        {
            var attachment = _mapper.MapFrom<ContentAttachment>(attachmentDto);
            attachment.CourseContentId = content.Id;
            content.Attachments.Add(attachment);
        }

        if (!dto.IsDraft && content.PublishedAt is null)
        {
            content.PublishedAt = DateTime.UtcNow;
        }

        content.UpdatedAt = DateTime.UtcNow;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.FindAsync(contentId, cancellationToken);
        if (content is null)
        {
            throw new BusinessException("Conteudo nao encontrado.", ECodigo.NaoEncontrado);
        }

        content.IsDraft = false;
        content.PublishedAt ??= DateTime.UtcNow;
        content.UpdatedAt = DateTime.UtcNow;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDisplayOrderAsync(Guid contentId, int displayOrder, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.FindAsync(contentId, cancellationToken);
        if (content is null)
        {
            throw new BusinessException("Conteudo nao encontrado.", ECodigo.NaoEncontrado);
        }

        content.DisplayOrder = displayOrder;
        content.UpdatedAt = DateTime.UtcNow;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }
}
