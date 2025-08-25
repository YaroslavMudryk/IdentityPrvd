using IdentityPrvd.DependencyInjection.Auth.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public static class IdentityPrvdAuthBuilderExtensions
{
    public static IIdentityPrvdBuilder UseExternalProviders(this IIdentityPrvdBuilder builder, Action<IExternalProvidersBuilder> configure)
    {
        if (configure != null)
        {
            var providersBuilder = new ExternalProvidersBuilder(builder);
            configure(providersBuilder);
        }

        // When using the fluent providers builder, assume explicit registration is done there
        return builder;
    }

    public static IIdentityPrvdBuilder UseExternalProviders(this IIdentityPrvdBuilder builder)
    {
        var externalBuilder = new ExternalProvidersBuilder(builder);

        externalBuilder
            .AddGoogle()
            .AddMicrosoft()
            .AddFacebook()
            .AddTwitter();

        return builder;
    }
}
