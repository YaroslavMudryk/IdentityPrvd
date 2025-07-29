namespace IdentityPrvd.Services.Security;

public interface IMfaService
{
    Task<bool> VerifyMfaAsync(string code, string secret = null);
}
