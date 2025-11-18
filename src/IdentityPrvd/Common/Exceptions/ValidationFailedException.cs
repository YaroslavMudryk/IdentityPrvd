using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Common.Exceptions;

public class ValidationFailedException : HttpResponseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationFailedException(Dictionary<string, string[]> errors)
        : base(StatusCodes.Status400BadRequest, "Validation errors")
    {
        Errors = errors;
    }
}

