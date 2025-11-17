namespace IdentityPrvd.Common.Exceptions;

public class NotFoundException : HttpResponseException
{
    public NotFoundException(string error) : base(404, error)
    {

    }

    public NotFoundException() : base(404, "Not Found")
    {

    }

    public NotFoundException(string localizationKey, params object[] args) : base(localizationKey, 404, args)
    {

    }
}
