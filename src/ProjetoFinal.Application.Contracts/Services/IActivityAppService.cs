using System;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IActivityAppService : IDefaultService<ActivityDto, ActivityCreateDto, ActivityUpdateDto, ActivityFilter, Guid>
{
}
