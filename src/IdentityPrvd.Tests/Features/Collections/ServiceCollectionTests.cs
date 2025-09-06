using FluentAssertions;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.DependencyInjection;
using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.DependencyInjection.Auth.Providers;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Tests.Features.Collections;

public class ServiceCollectionTests
{
    [Fact]
    public async Task AddIdentityPrvd_RegistersDefaultServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddIdentityPrvd(builder => builder.UseExternalProviders());

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IdentityPrvdOptions>().Should().NotBeNull();

        serviceProvider.GetService<ITransactionManager>().Should().BeOfType<EfCoreTransactionManager>();
        serviceProvider.GetService<IBanStore>().Should().BeOfType<EfBanStore>();
        serviceProvider.GetService<IBansQuery>().Should().BeOfType<EfBansQuery>();
        serviceProvider.GetService<IdentityPrvdContext>().Should().NotBeNull();

        serviceProvider.GetService<IHasher>().Should().BeOfType<FakeHasher>();
        serviceProvider.GetService<IProtectionService>().Should().BeOfType<FakeProtectionService>();

        serviceProvider.GetService<ISmsService>().Should().BeOfType<FakeSmsService>();
        serviceProvider.GetService<IEmailService>().Should().BeOfType<FakeEmailService>();
        serviceProvider.GetService<ILocationService>().Should().BeOfType<FakeLocationService>();

        serviceProvider.GetService<ExternalProviderManager>().Should().NotBeNull();
        serviceProvider.GetService<UserHelper>().Should().NotBeNull();
        serviceProvider.GetService<IMfaService>().Should().BeOfType<TotpMfaService>();
        serviceProvider.GetService<IAuthSchemes>().Should().BeOfType<DefaultAuthSchemes>();

        serviceProvider.GetService<ISessionManagerStore>().Should().BeOfType<InMemorySessionManagerStore>();

        var authProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
        var defaultScheme = await authProvider!.GetDefaultAuthenticateSchemeAsync();
        defaultScheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);

        var schemes = await authProvider.GetAllSchemesAsync();
        schemes.Select(s => s.Name).Should().ContainInConsecutiveOrder(
            [AppConstants.DefaultExternalProviderScheme, JwtBearerDefaults.AuthenticationScheme, "Google", "Microsoft", "Facebook", "Twitter"]);
        schemes.Count().Should().Be(6);
    }

    [Fact]
    public async Task AddIdentityPrvd_RegistersCustomExternalProviders()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddIdentityPrvd(builder =>
        {
            builder.UseExternalProviders(providers =>
            {
                providers.AddSteam().AddSpotify().AddGitHub();
            });
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var authProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
        var schemes = await authProvider!.GetAllSchemesAsync();
        schemes.Select(s => s.Name).Should().ContainInConsecutiveOrder(
            [AppConstants.DefaultExternalProviderScheme, JwtBearerDefaults.AuthenticationScheme, "Steam", "Spotify", "GitHub"]);
        schemes.Count().Should().Be(5);
    }

    [Fact]
    public void AddIdentityPrvd_RegistersCustomServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddIdentityPrvd(builder =>
        {
            builder.Options.Protection.Key = "0123456789ABCDEF0123456789ABCDEF";
            builder.UseAesProtectionService().UseSha512Hasher();
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IHasher>().Should().BeOfType<Sha512Hasher>();
        serviceProvider.GetService<IProtectionService>().Should().BeOfType<AesProtectionService>();
    }
}
