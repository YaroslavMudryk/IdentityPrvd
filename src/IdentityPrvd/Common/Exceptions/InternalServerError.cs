namespace IdentityPrvd.Common.Exceptions;

public class InternalServerError : HttpResponseException
{
    public InternalServerError(string error) : base(500, error)
    {

    }

    public InternalServerError() : base(500, "Internal Server Error")
    {

    }
}
