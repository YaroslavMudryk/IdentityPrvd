using IdentityPrvd.Options;

namespace IdentityPrvd.Common.Constants;

public static class HttpEndpoints
{
    public const string SignIn = "/api/identity/signin";
    public const string SignInMfa = "/api/identity/signin-mfa";
    public const string SignOut = "/api/identity/signout";
    public const string SignInOptions = "/api/identity/signin-options";
    public const string SignUp = "/api/identity/signup";
    public const string SignUpConfirm = "/api/identity/signup/confirm";
    public const string SignInExternal = "/api/identity/signin-external";
    public const string SignInExternalCallback = "/api/identity/signin-external-callback";
    public const string LinkedExternalSignin = "/api/identity/linked-external-signin";
    public const string LinkExternalSignin = "/api/identity/link-external-signin";
    public const string LinkExternalSigninCallback = "/api/identity/link-external-signin-callback";
    public const string UnlinkExternalSignin = "/api/identity/unlink-external-signin";
    public const string Sso = "/api/identity/sso";
    public const string WebSocketAuth = "/ws-auth";
    public const string Qr = "/api/identity/qr";
    public const string QrConfirm = "/api/identity/qr";
    public const string ChangeLogin = "/api/identity/change-login";
    public const string ChangePassword = "/api/identity/change-password";
    public const string StartResetPassword = "/api/identity/start-reset-password";
    public const string ResetPassword = "/api/identity/reset-password";
    public const string Initialize = "/api/identity/initialize";
    public const string RefreshToken = "/api/identity/refresh-token";
    public const string Sessions = "/api/identity/sessions";
    public const string RevokeSession = "/api/identity/revoke-sessions";
    public const string Mfa = "/api/identity/mfa";
    public const string Contacts = "/api/identity/contacts";
    public const string Devices = "/api/identity/devices";
    public const string DevicesVerify = "/api/identity/devices/verify";
    public const string DevicesUnverify = "/api/identity/devices/unverify";
    public const string Claims = "/api/identity/claims";
    public const string Clients = "/api/identity/clients";
    public const string Roles = "/api/identity/roles";

    public static Dictionary<string, EndpointOptions> Default = new()
    {
        { HttpActions.SignInAction, new EndpointOptions { Endpoint = SignIn, IsAvailable = true } },
        { HttpActions.SignInMfaAction, new EndpointOptions { Endpoint = SignInMfa, IsAvailable = true } }
    };
}
