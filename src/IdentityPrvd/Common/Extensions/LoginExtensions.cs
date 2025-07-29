using System.Text.RegularExpressions;

namespace IdentityPrvd.Common.Extensions;

public static class LoginExtensions
{
    public static bool IsEmail(string login)
    {
        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        return emailRegex.IsMatch(login);
    }

    public static bool IsPhone(string login)
    {
        var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
        return phoneRegex.IsMatch(login);
    }
}
