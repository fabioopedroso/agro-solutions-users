using Application.DTOs.Auth.Signature;

namespace Application.Contracts;
public interface IAuthService
{
    Task<string> GenerateToken(LoginDto login);
}
