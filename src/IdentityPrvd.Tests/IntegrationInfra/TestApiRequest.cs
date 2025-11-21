using IdentityPrvd.Helpers;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class TestApiRequest(string url, HttpMethod httpMethod)
{
    private readonly HttpRequestMessage _request = new(httpMethod, url);

    public static TestApiRequest Get(string url) => new(url, HttpMethod.Get);
    public static TestApiRequest Post(string url) => new(url, HttpMethod.Post);
    public static TestApiRequest Put(string url) => new(url, HttpMethod.Put);
    public static TestApiRequest Delete(string url) => new(url, HttpMethod.Delete);

    public HttpRequestMessage ToHttpRequestMessage() => _request;

    public Task<TestApiResponse> SendAsync(HttpClient client) => client.SendAsync(_request).ToApiResponse();

    public TestApiRequest WithPayload<T>(T payload)
    {
        _request.Content = JsonContent.Create(payload, options: Settings.Json);
        return this;
    }

    public TestApiRequest WithHeader(string key, string value)
    {
        _request.AddHeader(key, value);
        return this;
    }
}
