using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Infrastructure.Middleware;

public class LanguageDetectionMiddleware(
    IIdentityContext identityContext,
    ISessionsQuery sessionsQuery,
    IdentityPrvdOptions options) : IMiddleware
{
    private const string DefaultLanguage = "en";
    private const string AcceptLanguageHeader = "Accept-Language";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string language = DefaultLanguage;

        // Try to get language from session if authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var sessionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == IdentityClaims.Types.SessionId)?.Value;
            
            if (!string.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                var session = await sessionsQuery.GetSessionAsync(sessionId);
                if (session != null && !string.IsNullOrWhiteSpace(session.Language))
                {
                    language = session.Language;
                }
            }
        }

        // If not authenticated or no session language, try Accept-Language header
        if (language == DefaultLanguage && context.Request.Headers.ContainsKey(AcceptLanguageHeader))
        {
            var acceptLanguage = context.Request.Headers[AcceptLanguageHeader].ToString();
            language = ParseAcceptLanguageHeader(acceptLanguage, options);
        }

        identityContext.CurrentLanguage = language;
        await next(context);
    }

    private static string ParseAcceptLanguageHeader(string acceptLanguage, IdentityPrvdOptions options)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return DefaultLanguage;

        var supportedLanguages = options.Language.Languages ?? ["en", "uk"];
        
        // Parse Accept-Language header (e.g., "en-US,en;q=0.9,uk;q=0.8")
        var languages = acceptLanguage
            .Split(',')
            .Select(lang =>
            {
                var parts = lang.Trim().Split(';');
                var langCode = parts[0].Trim().Split('-')[0].ToLowerInvariant(); // Extract base language (e.g., "en" from "en-US")
                var quality = 1.0;
                
                if (parts.Length > 1 && parts[1].Trim().StartsWith("q="))
                {
                    if (double.TryParse(parts[1].Trim().Substring(2), out var q))
                        quality = q;
                }
                
                return new { Language = langCode, Quality = quality };
            })
            .Where(l => supportedLanguages.Contains(l.Language))
            .OrderByDescending(l => l.Quality)
            .Select(l => l.Language)
            .FirstOrDefault();

        return languages ?? DefaultLanguage;
    }
}
