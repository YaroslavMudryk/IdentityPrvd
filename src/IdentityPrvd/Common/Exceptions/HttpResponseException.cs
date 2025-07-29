namespace IdentityPrvd.Common.Exceptions;

public class HttpResponseException(int statusCode, string error = default!) : Exception(error)
{
    public int StatusCode { get; } = statusCode;

    public HttpResponseException() : this(500)
    {

    }

    public HttpResponseException(string error, int statusCode) : this(statusCode, error)
    {

    }
}
