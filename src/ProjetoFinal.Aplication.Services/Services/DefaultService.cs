using System.Reflection;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Exceptions;
using ProjetoFinal.Domain.Shared.Filters;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services;

public abstract class DefaultService<TEntity, TDto, TCreateDto, TUpdateDto, TFilter, TKey>(
    IDefaultRepository<TEntity, TFilter, TKey> repository,
    IUnityOfWork unityOfWork,
    IAutomapApi mapper) : IDefaultService<TDto, TCreateDto, TUpdateDto, TFilter, TKey>
    where TEntity : class
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilter : Filter, new()
{
    public virtual async Task<TDto> AddAsync(TCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = mapper.MapFrom<TEntity>(dto);
        var createdEntity = await repository.AddAsync(entity, cancellationToken);
        await unityOfWork.SaveChangesAsync(cancellationToken);
        var createdEntityDto = mapper.MapFrom<TDto>(createdEntity);
        return createdEntityDto;
    }

    public virtual async Task AddRangeAsync(IList<TCreateDto> dto,
        CancellationToken cancellationToken = default)
    {
        await repository.AddRangeAsync(mapper.MapFrom<List<TEntity>>(dto), cancellationToken);
        await unityOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
    }

    public virtual async Task<PagedResultDto<TDto>> GetAllAsync(TFilter filter,
        CancellationToken cancellationToken = default)
    {
        var entities = await repository
            .GetAllAsync(filter, cancellationToken);
        var dtos = mapper.MapFrom<PagedResultDto<TDto>>(entities);
        return dtos;
    }

    public virtual async Task<TDto> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var foundEntity = await repository.GetByIdAsync(id, cancellationToken);
        if (foundEntity is null)
            throw new RegistroNaoEncontradoException("Entidade não encontrada");
        var dto = mapper.MapFrom<TDto>(foundEntity);
        return dto;
    }

    public virtual async Task<TDto> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var foundEntity = await repository.FindAsync(id, cancellationToken);
        if (foundEntity is null)
            throw new RegistroNaoEncontradoException("Entidade não encontrada");
        var dto = mapper.MapFrom<TDto>(foundEntity);
        return dto;
    }

    public virtual async Task UpdateAsync(TUpdateDto dto, TKey id,
        CancellationToken cancellationToken = default)
    {
        TEntity? foundEntity = await repository.FindAsync(id, cancellationToken);
        if (foundEntity is null)
            throw new RegistroNaoEncontradoException("Entidade não encontrada");
        mapper.MapTo(dto, foundEntity);
        PropertyInfo? propertyInfo = foundEntity.GetType().GetProperty("UpdatedAt");
        if (propertyInfo is not null)
            propertyInfo.SetValue(foundEntity, DateTime.UtcNow);
        await repository.UpdateAsync(foundEntity);
        await unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TDto> DeleteAsync(TKey id,
        CancellationToken cancellationToken = default)
    {
        TEntity? foundEntity = await repository.FindAsync(id, cancellationToken);
        if (foundEntity is null)
            throw new RegistroNaoEncontradoException("Entidade não encontrada");
        var deleted = await repository.DeleteAsync(foundEntity, cancellationToken);
        await unityOfWork.SaveChangesAsync(cancellationToken);
        var dto = mapper.MapFrom<TDto>(deleted);
        return dto;
    }

    public virtual Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
    {
        return repository.HasAnyAsync(null, cancellationToken);
    }
}
