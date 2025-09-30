using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
public abstract class BaseController<
    TDto,
    TCreateDto,
    TUpdateDto,
    TFilter,
    TKey,
    TService
>(TService service) : ControllerBase
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilter : Filter
    where TService : IDefaultService<
        TDto,
        TCreateDto,
        TUpdateDto,
        TFilter,
        TKey>
{
    protected TService Service { get; } = service;

    [HttpPost]
    public virtual async Task<TDto> AddAsync(
        [FromBody] TCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        var entity = await Service.AddAsync(createDto, cancellationToken);
        return entity;
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<TDto>> GetAllAsync(
        [FromQuery] TFilter filter,
        CancellationToken cancellationToken = default)
    {
        var result = await Service.GetAllAsync(filter, cancellationToken);
        return result;
    }

    [HttpGet("{id}")]
    public virtual async Task<TDto> GetByIdAsync(
        [FromRoute] TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Service.GetByIdAsync(id, cancellationToken);
        return entity;
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> UpdateAsync(
        [FromRoute] TKey id,
        [FromBody] TUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await Service.UpdateAsync(dto, id, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public virtual async Task<TDto> DeleteAsync(
        [FromRoute] TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Service.DeleteAsync(id, cancellationToken);
        return entity;
    }
}
