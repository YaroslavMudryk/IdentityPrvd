using IdentityPrvd.WebApi.Exceptions;

namespace IdentityPrvd.WebApi.UserContext;

public class UserContext : IUserContext
{
    public CurrentUser CurrentUser { get; set; } = UninitializedUser.Instance;

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
