using Application.Contracts;
using Application.DTOs.Auth.Signature;
using Application.Exceptions;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services.Authentication;
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public AuthService(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public async Task<string> GenerateToken(LoginDto login)
    {
        var user = await GetValidatedUserAsync(login);
        var claims = GenerateClaims(user);
        var token = CreateJwtToken(claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #region MétodosPrivados
    private async Task<User> GetValidatedUserAsync(LoginDto login)
    {
        var user = await ValidateUserAsync(login);
        await UpdatePasswordIfNeededAsync(user, login.Password);
        return user;
    }

    private async Task<User> ValidateUserAsync(LoginDto login)
    {
        var email = new Email(login.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            throw new NotFoundException("O usuário não foi encontrado.");

        if (!user.VerifyPassword(login.Password))
            throw new UnauthorizedException("Usuário ou senha inválidos.");

        return user;
    }

    private async Task UpdatePasswordIfNeededAsync(User user, string password)
    {
        var passwordVerificationResult = user.GetPasswordVerificationResult(password);
        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.ForceChangePassword(new Password(password));
            await _userRepository.UpdateAsync(user);
        }
    }

    private IEnumerable<Claim> GenerateClaims(User user)
    {
        return new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("Email", user.Email.Address),
        };
    }

    private JwtSecurityToken CreateJwtToken(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials
        );
    }
    #endregion MétodosPrivados
}
