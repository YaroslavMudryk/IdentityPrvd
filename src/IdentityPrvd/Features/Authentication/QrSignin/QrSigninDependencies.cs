using FluentValidation;
using IdentityPrvd.Features.Authentication.QrSignin.Dtos;
using IdentityPrvd.Features.Authentication.QrSignin.Dtos.Validators;
using IdentityPrvd.Features.Authentication.QrSignin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.QrSignin;

public static class QrSigninDependencies
{
    public static void AddQrSigninDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IValidator<QrRequestDto>, QrRequestDtoValidator>();
    }
}
