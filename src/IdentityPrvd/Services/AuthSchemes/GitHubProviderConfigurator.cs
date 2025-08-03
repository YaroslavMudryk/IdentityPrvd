using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class GitHubProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "GitHub";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddGitHub(githubOptions =>
        {
            githubOptions.ClientId = options.ClientId;
            githubOptions.ClientSecret = options.ClientSecret;
            githubOptions.CallbackPath = options.CallbackPath ?? "/signin-github";
            githubOptions.SignInScheme = "cookie";
            githubOptions.SaveTokens = true;

            // Default GitHub scopes
            if (!options.Scopes.Any())
            {
                githubOptions.Scope.Add("read:user");
                githubOptions.Scope.Add("user:email");
            }
            else
            {
                foreach (var scope in options.Scopes)
                {
                    githubOptions.Scope.Add(scope);
                }
            }
        });
    }
} 