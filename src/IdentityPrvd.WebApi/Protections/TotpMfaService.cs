using OtpNet;

namespace IdentityPrvd.WebApi.Protections;

public class TotpMfaService(IProtectionService protectionService) : IMfaService
{
    public async Task<bool> VerifyMfaAsync(string code, string secret = null)
    {
        var totp = new Totp(Base32Encoding.ToBytes(protectionService.DecryptData(secret)));
        return await Task.FromResult(totp.VerifyTotp(code, out var _));
    }
}
