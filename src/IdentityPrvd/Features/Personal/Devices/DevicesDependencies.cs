using IdentityPrvd.Features.Personal.Devices.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Personal.Devices;

public static class DevicesDependencies
{
    public static IServiceCollection AddDevicesDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetDevicesOrchestrator>();
        services.AddScoped<VerifyDeviceOrchestrator>();
        services.AddScoped<UnverifyDeviceOrchestrator>();
        services.AddScoped<DeleteDeviceOrchestrator>();

        return services;
    }
}
