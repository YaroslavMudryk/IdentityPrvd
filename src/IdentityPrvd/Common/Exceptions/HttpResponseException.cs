namespace IdentityPrvd.Common.Exceptions;

public class HttpResponseException(int statusCode, string error = default!) : Exception(error)
{
    public int StatusCode { get; } = statusCode;
    public string? LocalizationKey { get; set; }
    public object[]? LocalizationArgs { get; set; }

    public HttpResponseException() : this(500)
    {

    }

    public HttpResponseException(string error, int statusCode) : this(statusCode, error)
    {

    }

    public HttpResponseException(string localizationKey, int statusCode, params object[] args) : this(statusCode, localizationKey)
    {
        LocalizationKey = localizationKey;
        LocalizationArgs = args;
    }
}
