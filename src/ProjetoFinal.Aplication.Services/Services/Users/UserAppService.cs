using System;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Users;

public class UserAppService : DefaultService<User, UserDto, UserCreateDto, UserUpdateDto, UserFilter, Guid>, IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IAutomapApi _mapper;

    public UserAppService(
        IUserRepository userRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(userRepository, unityOfWork, mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public override async Task<UserDto> AddAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.FirstOrDefaultByPredicateAsync(
            user => user.Email == normalizedEmail,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new BusinessException("J? existe um usu?rio cadastrado com este e-mail.", ECodigo.Conflito);
        }

        dto.ExternalId ??= Guid.NewGuid();
        dto.Email = normalizedEmail;

        return await base.AddAsync(dto, cancellationToken);
    }

    public override async Task UpdateAsync(UserUpdateDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindAsync(id, cancellationToken);
        if (user is null)
        {
            throw new BusinessException("Usu?rio n?o encontrado.", ECodigo.NaoEncontrado);
        }

        await base.UpdateAsync(dto, id, cancellationToken);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.FirstOrDefaultByPredicateAsync(
            entity => entity.Email == normalizedEmail,
            cancellationToken);

        return user is null ? null : _mapper.MapFrom<UserDto>(user);
    }
}
