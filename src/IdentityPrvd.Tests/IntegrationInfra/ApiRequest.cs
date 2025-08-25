using IdentityPrvd.Helpers;
using System.Net.Http.Json;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class ApiRequest(string url, HttpMethod httpMethod)
{
    private readonly HttpRequestMessage _request = new(httpMethod, url);

    public static ApiRequest Get(string url) => new(url, HttpMethod.Get);
    public static ApiRequest Post(string url) => new(url, HttpMethod.Post);
    public static ApiRequest Put(string url) => new(url, HttpMethod.Put);
    public static ApiRequest Delete(string url) => new(url, HttpMethod.Delete);

    public HttpRequestMessage ToHttpRequestMessage() => _request;

    public Task<ApiResponse> SendAsync(HttpClient client) => client.SendAsync(_request).ToApiResponse();

    public ApiRequest WithPayload<T>(T payload)
    {
        _request.Content = JsonContent.Create(payload, options: Settings.Json);
        return this;
    }

    public ApiRequest WithHeader(string key, string value)
    {
        _request.AddHeader(key, value);
        return this;
    }
}
