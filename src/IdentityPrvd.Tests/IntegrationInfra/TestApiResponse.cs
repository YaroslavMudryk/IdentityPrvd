using IdentityPrvd.Helpers;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class TestApiResponse(HttpResponseMessage responseMessage)
{
    public HttpResponseMessage Response { get; } = responseMessage;
    public string GetBodyAsString() => Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    public T GetBody<T>() => Response.Content.ReadFromJsonAsync<T>(Settings.Json).GetAwaiter().GetResult()!;
    public int StatusCode => (int)Response.StatusCode;
}
