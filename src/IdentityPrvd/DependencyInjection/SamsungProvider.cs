using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Security.Claims;

namespace IdentityPrvd.DependencyInjection;

// Example custom external provider with fictional endpoints
public sealed class SamsungProvider : ICustomExternalProvider
{
	public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
	{
		// Uses values from identityOptions.ExternalProviders["Samsung"] if present
		authBuilder.AddExternalOAuthFromOptions(identityOptions, "Samsung", o =>
		{
			// Fictional endpoints
			o.AuthorizationEndpoint = "https://account.samsung.com/iam/oauth2/authorize";
			o.TokenEndpoint = "https://account.samsung.com/iam/oauth2/token";
			o.UserInformationEndpoint = "https://api.samsung.com/v1/me";

			// Map standard claims
			o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
			o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
			o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

			o.Events = new OAuthEvents
			{
				OnCreatingTicket = async context =>
				{
					using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
					using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
					response.EnsureSuccessStatusCode();
					using var payload = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted), cancellationToken: context.HttpContext.RequestAborted);
					context.RunClaimActions(payload.RootElement);
				}
			};
		});
	}
}


