using IdentityPrvd.Features.Personal.Contacts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Personal.Contacts;

public static class ContactsDependencies
{
    public static IServiceCollection AddContactsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetContactsOrchestrator>();
        services.AddScoped<CreateContactOrchestrator>();
        services.AddScoped<DeleteContactOrchestrator>();

        return services;
    }
}
