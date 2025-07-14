namespace IdentityPrvd.WebApi.Db.Entities.Internal;

public class ClientAppInfo
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Image { get; set; }
}
