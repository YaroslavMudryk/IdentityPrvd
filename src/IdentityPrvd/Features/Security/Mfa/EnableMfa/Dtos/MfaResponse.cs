namespace IdentityPrvd.Features.Security.Mfa.EnableMfa.Dtos;

public class MfaResponse
{
    public string SetupUrl { get; set; }
    public string SetupCode { get; set; }
    public IReadOnlyList<string> RestoreCodes { get; set; }
}
