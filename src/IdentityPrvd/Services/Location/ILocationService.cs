using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Services.Location;

public interface ILocationService
{
    Task<LocationInfo> GetIpInfoAsync(string ip);
}
