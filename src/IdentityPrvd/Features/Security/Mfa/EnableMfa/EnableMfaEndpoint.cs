using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Mfa.EnableMfa.Dtos;
using IdentityPrvd.Features.Security.Mfa.EnableMfa.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Mfa.EnableMfa;

public class EnableMfaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/mfa",
            async (MfaDto dto, EnableMfaOrchestrator orc) =>
            {
                var mfaResult = await orc.EnableMfaAsync(dto);
                return mfaResult is null ?
                    Results.NoContent() :
                    Results.Ok(mfaResult.MapToResponse());
            }).WithTags("Mfa");
    }
}
