namespace IdentityPrvd.WebApi.Exceptions;

public class NotFoundException : HttpResponseException
{
    public NotFoundException(string error) : base(404, error)
    {

    }

    public NotFoundException() : base(404, "Not Found")
    {

    }
}
