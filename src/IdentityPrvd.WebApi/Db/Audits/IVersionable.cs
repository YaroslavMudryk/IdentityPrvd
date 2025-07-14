namespace IdentityPrvd.WebApi.Db.Audits;

public interface IVersionable
{
    public int Version { get; set; }
}
