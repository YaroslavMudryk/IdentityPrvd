namespace IdentityPrvd.WebApi.Protections;

public interface IMfaService
{
    Task<bool> VerifyMfaAsync(string code, string secret = null);
}
