namespace IdentityPrvd.WebApi.SmsSender;

public interface ISmsService
{
    Task SendSmsAsync(string phone, string message);
}

public class FakeSmsService : ISmsService
{
    public Task SendSmsAsync(string phone, string message)
    {
        return Task.CompletedTask;
    }
}
