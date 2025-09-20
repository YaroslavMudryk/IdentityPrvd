using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Personal.Devices.Dtos;
using IdentityPrvd.Features.Personal.Devices.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Personal.Devices;

public class GetDevicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/devices",
            async (GetDevicesOrchestrator orc) =>
            {
                var devices = await orc.GetDevicesAsync();
                return Results.Ok(devices.MapToResponse());
            }).WithTags("Devices");
    }
}

public class VerifyDeviceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/devices/verify",
            async (VerifyDeviceDto dto, VerifyDeviceOrchestrator orc) =>
            {
                var verifiedDevice = await orc.VerifyDeviceAsync(dto);
                return Results.Ok(verifiedDevice.MapToResponse());
            }).WithTags("Devices");
    }
}

public class UnverifyDeviceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/devices/unverify/{deviceId}",
            async (Ulid deviceId, UnverifyDeviceOrchestrator orc) =>
            {
                await orc.UnverifyDeviceAsync(deviceId);
                return Results.NoContent();
            }).WithTags("Devices");
    }
}

public class DeleteDeviceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/devices/{deviceId}",
            async (Ulid deviceId, DeleteDeviceOrchestrator orc) =>
            {
                await orc.DeleteDeviceAsync(deviceId);
                return Results.NoContent();
            }).WithTags("Devices");
    }
}
