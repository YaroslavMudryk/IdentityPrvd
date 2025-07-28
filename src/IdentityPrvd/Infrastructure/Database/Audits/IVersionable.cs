namespace IdentityPrvd.Infrastructure.Database.Audits;

public interface IVersionable
{
    public int Version { get; set; }
}
