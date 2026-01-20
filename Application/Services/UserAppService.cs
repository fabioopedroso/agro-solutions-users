using Application.Contracts;
using Application.DTOs.User.Signature;
using Application.Exceptions;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;

namespace Application.Services;
public class UserAppService 
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser; 

    public UserAppService(IUserRepository userRepository, ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser; 
    }

    public async Task Register(RegisterDto dto)
    {
        await EnsureEmailIsUnique(dto.Email);
        var user = BuildUser(dto);
        await _userRepository.AddAsync(user);
    }

    public async Task ChangePassword(ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId);

        if (user is null)
            throw new NotFoundException("Usuário não encontrado.");

        user.ChangePassword(dto.Password, dto.NewPassword);

        await _userRepository.UpdateAsync(user);
    }

    private User BuildUser(RegisterDto signature)
    {
        var email = new Email(signature.Email);
        var password = new Password(signature.Password);
        return new User(email, password);
    }

    #region MétodosPrivados
    private async Task EnsureEmailIsUnique(string emailString)
    {
        var email = new Email(emailString);
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user != null)
            throw new ConflictException("O E-mail informado já está cadastrado.");
    }
    #endregion MétodosPrivados
}
