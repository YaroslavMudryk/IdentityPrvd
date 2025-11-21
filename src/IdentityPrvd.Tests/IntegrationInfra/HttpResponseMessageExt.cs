namespace IdentityPrvd.Tests.IntegrationInfra;

public static class HttpResponseMessageExt
{
    public static void AddHeader<T>(this HttpRequestMessage request, string key, T value)
        => request!.Headers.Add(key, value.ToString());

    public static async Task<TestApiResponse> ToApiResponse(this Task<HttpResponseMessage> response) => new(await response);
    public static TestApiResponse ToApiResponse(this HttpResponseMessage response) => new(response);
}
