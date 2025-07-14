using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.AspNetCore.Authorization;

namespace IdentityPrvd.WebApi.ServerSideSessions;

public class ServerSideSessionMiddleware(
    ISessionManager sessionManager,
    IUserContext userContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(sessionManager);
        ArgumentNullException.ThrowIfNull(userContext);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (RequiresAuthentication(context))
        {
            if (context.User.Identity!.IsAuthenticated)
            {
                var sessionId = context.User.Claims.FirstOrDefault(c => c.Type == IdentityClaims.Types.SessionId)?.Value;
                var userId = context.User.Claims.FirstOrDefault(c => c.Type == IdentityClaims.Types.UserId)?.Value;

                if (sessionId != null && userId != null)
                {
                    var isActive = await sessionManager.IsActiveSessionAsync(userId, sessionId);
                    if (isActive)
                    {
                        if (((UserContext.UserContext)userContext).CurrentUser is UninitializedUser)
                        {
                            var user = context.User.GetCurrentUser(await sessionManager.GetSessionPermissionsAsync(sessionId));
                            ((UserContext.UserContext)userContext).CurrentUser = user;
                        }
                        await next(context);

                        var options = context.RequestServices.GetRequiredService<IdentityPrvdOptions>();
                        if (options.TrackSessionActivity)
                        {
                            await sessionManager.MarkSessionLastActivityAsync(userId, sessionId);
                        }
                    }
                    else
                    {
                        await RespondUnauthorizedAsync(context);
                    }
                }
                else
                {
                    await RespondUnauthorizedAsync(context);
                }
            }
            else
            {
                await RespondUnauthorizedAsync(context);
            }
        }
        else
        {
            ((UserContext.UserContext)userContext).CurrentUser = ServiceUser.Instance;
            await next(context);
        }
    }

    private static bool RequiresAuthentication(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() == null;
    }

    private static async Task RespondUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Unauthorized"));
    }
}

public static class ServerSideSessionMiddlewareExtensions
{
    public static void UseServerSideSessions(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ServerSideSessionMiddleware>();
    }
}
