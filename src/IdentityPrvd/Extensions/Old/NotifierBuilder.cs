using IdentityPrvd.Options;
using IdentityPrvd.Services.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Extensions.Old;

/// <summary>
/// Builder for notification services with support for different implementations
/// </summary>
public class NotifierBuilder
{
    private readonly IServiceCollection _services;

    public NotifierBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Add email service with fake implementation (default)
    /// </summary>
    public NotifierBuilder AddEmail()
    {
        _services.AddScoped<IEmailService, FakeEmailService>();
        return this;
    }

    /// <summary>
    /// Add SMS service with fake implementation (default)
    /// </summary>
    public NotifierBuilder AddSms()
    {
        _services.AddScoped<ISmsService, FakeSmsService>();
        return this;
    }

    /// <summary>
    /// Add all notification services with fake implementations (default)
    /// </summary>
    public NotifierBuilder AddAll()
    {
        return AddEmail().AddSms();
    }

    /// <summary>
    /// Use custom email service implementation
    /// </summary>
    public NotifierBuilder UseCustomEmailService<TEmailService>() where TEmailService : class, IEmailService
    {
        _services.AddScoped<IEmailService, TEmailService>();
        return this;
    }

    /// <summary>
    /// Use custom SMS service implementation
    /// </summary>
    public NotifierBuilder UseCustomSmsService<TSmsService>() where TSmsService : class, ISmsService
    {
        _services.AddScoped<ISmsService, TSmsService>();
        return this;
    }

    /// <summary>
    /// Use Twilio SMS service
    /// </summary>
    public NotifierBuilder UseTwilioSms(string accountSid, string authToken, string fromNumber)
    {
        // This would be implemented when you create a Twilio SMS service
        // _services.AddScoped<ISmsService, TwilioSmsService>();
        // _services.Configure<TwilioOptions>(options =>
        // {
        //     options.AccountSid = accountSid;
        //     options.AuthToken = authToken;
        //     options.FromNumber = fromNumber;
        // });
        return this;
    }

    /// <summary>
    /// Use SendGrid email service
    /// </summary>
    public NotifierBuilder UseSendGridEmail(string apiKey, string fromEmail, string fromName)
    {
        // This would be implemented when you create a SendGrid email service
        // _services.AddScoped<IEmailService, SendGridEmailService>();
        // _services.Configure<SendGridOptions>(options =>
        // {
        //     options.ApiKey = apiKey;
        //     options.FromEmail = fromEmail;
        //     options.FromName = fromName;
        // });
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, new IdentityPrvdOptions());
    }
}
