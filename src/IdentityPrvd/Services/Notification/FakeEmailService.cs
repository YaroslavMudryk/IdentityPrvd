namespace IdentityPrvd.Services.Notification;

public class FakeEmailService : IEmailService
{
    public static List<(string, string, string, string)> Emails = new();

    public Task SendEmailAsync(string emailTo, string subject, string plainContent, string htmlContent = null)
    {
        Emails.Add((emailTo, subject, plainContent, htmlContent));
        return Task.CompletedTask;
    }
}
