using Application.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services.Identity;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out var id))
                return id;

            throw new InvalidOperationException("Usuário não autenticado ou ID inválido.");
        }
    }

    public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst("Email")?.Value ?? string.Empty;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
