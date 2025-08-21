using IdentityPrvd.Endpoints;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.AspNetCore.Builder;

namespace IdentityPrvd.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UseIdentityPrvd(this WebApplication app)
    {
        app.UseMiddleware<CorrelationContextMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseServerSideSessions();

        app.MapEndpoints();
        return app;
    }
}
