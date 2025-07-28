namespace IdentityPrvd.Services.Notification;

public class FakeSmsService : ISmsService
{
    public Task SendSmsAsync(string phone, string message)
    {
        return Task.CompletedTask;
    }
}
