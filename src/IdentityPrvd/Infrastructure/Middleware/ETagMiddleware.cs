using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;

namespace IdentityPrvd.Infrastructure.Middleware;

public class ETagMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method == "GET")
            await InvokeWithEtagAsync(context, next);
        else
            await next(context);
    }

    private static async Task InvokeWithEtagAsync(HttpContext context, RequestDelegate next)
    {
        var response = context.Response;
        var originalStream = response.Body;

        using (var ms = new MemoryStream())
        {
            response.Body = ms;
            await next(context);

            if (context.Response.StatusCode != StatusCodes.Status200OK)
                return;

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
                return;

            if (response.Body.Length > 1024 * 1024 * 2) //limit 2 Mb
                return;

            ms.Position = 0;
            string checksum = HashingHelper.CalculateHash(ms);

            response.Headers[HeaderNames.ETag] = checksum;

            if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag) && checksum == etag)
            {
                response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            ms.Position = 0;
            await ms.CopyToAsync(originalStream);
        }
    }
}

public static class HashingHelper
{
    public static string CalculateHash(MemoryStream ms)
    {
        string checksum = "";

        using (var algo = SHA1.Create())
        {
            byte[] bytes = algo.ComputeHash(ms);
            checksum = $"W/\"{WebEncoders.Base64UrlEncode(bytes)}\"";
        }

        return checksum;
    }
}
