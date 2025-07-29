namespace IdentityPrvd.Services.Notification;

public interface IEmailService
{
    Task SendEmailAsync(string emailTo, string subject, string plainContent, string htmlContent = null);
}
