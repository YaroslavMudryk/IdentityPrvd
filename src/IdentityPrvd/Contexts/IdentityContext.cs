using IdentityPrvd.Common.Exceptions;

namespace IdentityPrvd.Contexts;

public class IdentityContext : IIdentityContext
{
    public CurrentUser CurrentUser { get; set; } = UninitializedUser.Instance;
    public string IpAddress { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;

    public TUser AssumeAuthenticated<TUser>() where TUser : CurrentUser
    {
        if (CurrentUser is TUser user)
            return user;

        throw new UnauthorizedException();
    }

    public string GetBy<TUser>() where TUser : CurrentUser
    {
        if (CurrentUser is BasicAuthenticatedUser user)
            return user.UserId;

        if (CurrentUser is ServiceUser serviceUser)
            return serviceUser.System;

        throw new UnauthorizedException();
    }
}
