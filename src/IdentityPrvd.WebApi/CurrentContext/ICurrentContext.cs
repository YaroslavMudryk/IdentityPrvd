namespace IdentityPrvd.WebApi.CurrentContext;

public interface ICurrentContext
{
    public string IpAddress { get; }
    public string CorrelationId { get; }
}
