using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IdentityPrvd.Common.Constants;

public static class AppConstants
{
    public static string DefaultExternalProviderScheme => "Cookie";
    public static string DefaultScheme => JwtBearerDefaults.AuthenticationScheme;
}
