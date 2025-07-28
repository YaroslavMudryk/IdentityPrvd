using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Services.Security;

public interface IUserSecureService
{
    Task IncrementFailedLoginByBlockAsync(IdentityUser user, DateTime utcNow);
    Task IncrementFailedLoginByPasswordAsync(IdentityUser user, DateTime utcNow);
}
