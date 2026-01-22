using Core.Interfaces.Repositories;
using System.Security.Claims;

namespace AgroSolutions.Users.Middlewares;

public class ValidateUserExistsMiddleware
{
    private readonly RequestDelegate _next;

    public ValidateUserExistsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if(!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if(userId is null || !int.TryParse(userId.Value, out var id))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token inválido.");
            return;
        }

        var user = await userRepository.GetByIdAsync(id);
        if(user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Usuário não existe.");
            return;
        }

        await _next(context);
    }
}
