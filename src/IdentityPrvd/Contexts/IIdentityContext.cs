namespace IdentityPrvd.Contexts;

public interface IIdentityContext
{
    CurrentUser CurrentUser { get; set; }
    string IpAddress { get; set; }
    string CorrelationId { get; set; }
    TUser AssumeAuthenticated<TUser>() where TUser : CurrentUser;
    string GetBy<TUser>() where TUser : CurrentUser;
}
