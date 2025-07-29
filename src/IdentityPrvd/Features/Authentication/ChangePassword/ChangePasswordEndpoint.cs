using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ChangePassword.Dtos;
using IdentityPrvd.Features.Authentication.ChangePassword.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/change-password",
             async (ChangePasswordDto dto, ChangePasswordOrchestrator orc) =>
             {
                 await orc.ChangePasswordAsync(dto);
                 return Results.NoContent();
             });
    }
}
