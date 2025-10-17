using System;
using System.Globalization;
using System.Text;
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

public class CourseAppService : DefaultService<Course, CourseDto, CourseCreateDto, CourseUpdateDto, CourseFilter, Guid>, ICourseAppService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public CourseAppService(
        ICourseRepository courseRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(courseRepository, unityOfWork, mapper)
    {
        _courseRepository = courseRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public override async Task<CourseDto> AddAsync(CourseCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.MapFrom<Course>(dto);
        entity.Title = dto.Title.Trim();
        entity.CategoryName = dto.CategoryName?.Trim() ?? string.Empty;
        entity.Slug = await GenerateUniqueSlugAsync(entity.Title, cancellationToken);
        entity.EnableChat = entity.Mode == CourseMode.InteractiveClasses ? true : dto.EnableChat;
        entity.EnableForum = dto.EnableForum;
        entity.IsPublished = false;
        entity.InstructorId = dto.InstructorId;

        var createdEntity = await _courseRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.MapFrom<CourseDto>(createdEntity);
    }

    public override async Task UpdateAsync(CourseUpdateDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _courseRepository.FindAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new BusinessException("Curso nao encontrado.", ECodigo.NaoEncontrado);
        }

        var previousTitle = entity.Title;
        _mapper.MapTo(dto, entity);

        entity.CategoryName = dto.CategoryName?.Trim() ?? string.Empty;
        if (!string.Equals(previousTitle, entity.Title, StringComparison.OrdinalIgnoreCase))
        {
            entity.Slug = await GenerateUniqueSlugAsync(entity.Title, cancellationToken);
        }

        entity.EnableChat = entity.Mode == CourseMode.InteractiveClasses ? true : dto.EnableChat;
        entity.EnableForum = dto.EnableForum;
        entity.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseSummaryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.FirstOrDefaultByPredicateAsync(
            entity => entity.Slug == slug,
            cancellationToken);
        return course is null ? null : _mapper.MapFrom<CourseSummaryDto>(course);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var normalizedTitle = RemoveDiacritics(title.ToLowerInvariant().Trim());
        var slugBuilder = new StringBuilder();
        foreach (var character in normalizedTitle)
        {
            if (char.IsLetterOrDigit(character))
            {
                slugBuilder.Append(character);
            }
            else if (char.IsWhiteSpace(character) || character == '-' || character == '_')
            {
                if (slugBuilder.Length == 0 || slugBuilder[^1] == '-')
                {
                    continue;
                }
                slugBuilder.Append('-');
            }
        }

        var baseSlug = slugBuilder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = Guid.NewGuid().ToString()[..8];
        }

        var candidate = baseSlug;
        var suffix = 1;
        while (await _courseRepository.SlugExistsAsync(candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        return candidate;
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
