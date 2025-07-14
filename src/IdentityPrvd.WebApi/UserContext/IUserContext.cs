namespace IdentityPrvd.WebApi.UserContext;

public interface IUserContext
{
    CurrentUser CurrentUser { get; }

    TUser AssumeAuthenticated<TUser>() where TUser : CurrentUser;

    string GetBy<TUser>() where TUser : CurrentUser;
}
