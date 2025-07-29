namespace IdentityPrvd.Contexts;

public class CurrentContext : ICurrentContext
{
    public string IpAddress { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;
}
