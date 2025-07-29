namespace IdentityPrvd.Services.Notification;

public class FakeEmailService : IEmailService
{
    public Task SendEmailAsync(string emailTo, string subject, string plainContent, string htmlContent = null)
    {
        return Task.CompletedTask;
    }
}
