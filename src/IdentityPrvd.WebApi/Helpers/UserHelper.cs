using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Helpers;

public class UserHelper(
    TimeProvider timeProvider)
{
    public bool IsBlocked(IdentityUser user)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var isLocked = false;
        if (user.BlockedUntil == null)
            isLocked = false;
        if (user.BlockedUntil.HasValue)
        {
            if (user.BlockedUntil.Value > utcNow)
                isLocked = true;
            else
            {
                user.BlockedUntil = null;
                isLocked = false;
            }
        }
        return isLocked;
    }
}
