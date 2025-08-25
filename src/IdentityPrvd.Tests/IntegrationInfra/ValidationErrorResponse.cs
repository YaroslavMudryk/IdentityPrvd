namespace IdentityPrvd.Tests.IntegrationInfra;

internal class ValidationErrorResponse
{
    public required Dictionary<string, List<string>> Errors { get; set; }
}
