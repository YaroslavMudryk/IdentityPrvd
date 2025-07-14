using Redis.OM.Modeling;
using System.Text.Json.Serialization;

namespace IdentityPrvd.WebApi.ServerSideSessions;

[Document(IndexName = "session-idx", Prefixes = ["sessions"], StorageType = StorageType.Json)]
public class SessionInfo
{
    [RedisIdField]
    [Indexed]
    public string SessionId { get; set; }
    [Indexed]
    public string UserId { get; set; }
    public long CreatedAtUnix { get; set; }
    public long SessionExpireUnix { get; set; }
    public long? LastAccessedAtUnix { get; set; }
    public Dictionary<string, List<string>> Permissions { get; set; } = [];

    [JsonIgnore]
    public DateTime? LastAccessedAt
    {
        get => LastAccessedAtUnix.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(LastAccessedAtUnix.Value).UtcDateTime
            : null;

        set => LastAccessedAtUnix = value.HasValue
            ? new DateTimeOffset(value.Value).ToUnixTimeSeconds()
            : null;
    }

    [JsonIgnore]
    public DateTime CreatedAt
    {
        get => DateTimeOffset.FromUnixTimeSeconds(CreatedAtUnix).UtcDateTime;
        set => CreatedAtUnix = new DateTimeOffset(value).ToUnixTimeSeconds();
    }

    [JsonIgnore]
    public DateTime SessionExpire
    {
        get => DateTimeOffset.FromUnixTimeSeconds(SessionExpireUnix).UtcDateTime;
        set => SessionExpireUnix = new DateTimeOffset(value).ToUnixTimeSeconds();
    }
}
