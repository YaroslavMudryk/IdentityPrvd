namespace IdentityPrvd.Contexts;

public interface ICurrentContext
{
    public string IpAddress { get; }
    public string CorrelationId { get; }
}
