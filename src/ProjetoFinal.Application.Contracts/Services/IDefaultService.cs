using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IDefaultService<TDto, TCreateDto, TUpdateDto, TFilter, TKey>
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilter : Filter
{
    public Task<TDto> AddAsync(TCreateDto dto, CancellationToken cancellationToken = default);
    public Task AddRangeAsync(IList<TCreateDto> dto, CancellationToken cancellationToken = default);
    public Task<PagedResultDto<TDto>> GetAllAsync(TFilter filter, CancellationToken cancellationToken = default);
    public Task<TDto> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public Task<TDto> FindAsync(TKey id, CancellationToken cancellationToken = default);
    public Task UpdateAsync(TUpdateDto dto, TKey id, CancellationToken cancellationToken = default);
    public Task<TDto> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);
}
