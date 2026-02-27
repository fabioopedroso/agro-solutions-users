using System.Text.Json;

namespace AgroSolutions.Users.Middlewares;

public static class StatusCodePagesExtensions
{
    public static IApplicationBuilder UseCustomStatusCodePages(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            var statusCode = response.StatusCode;

            if (statusCode == StatusCodes.Status401Unauthorized || statusCode == StatusCodes.Status403Forbidden)
            {
                response.ContentType = "application/problem+json";

                var problemDetails = new
                {
                    type = statusCode == 401 ? "https://httpstatuses.com/401" : "https://httpstatuses.com/403",
                    title = statusCode == 401 ? "Unauthorized" : "Forbidden",
                    status = statusCode,
                    detail = statusCode == 401
                        ? "Autenticação é necessária para acessar este recurso."
                        : "Você não tem permissão para acessar este recurso.",
                    instance = context.HttpContext.Request.Path
                };

                await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
        });

        return app;
    }
}
