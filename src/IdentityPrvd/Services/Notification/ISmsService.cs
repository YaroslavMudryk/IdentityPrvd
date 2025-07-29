namespace IdentityPrvd.Services.Notification;

public interface ISmsService
{
    Task SendSmsAsync(string phone, string message);
}
