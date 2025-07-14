namespace IdentityPrvd.WebApi.Exceptions;

public class UnauthorizedException : HttpResponseException
{
    public UnauthorizedException(string error) : base(401, error)
    {

    }

    public UnauthorizedException() : base(401, "Unauthorized")
    {

    }
}
