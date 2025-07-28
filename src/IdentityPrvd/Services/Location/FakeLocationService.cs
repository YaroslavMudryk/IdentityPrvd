using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Services.Location;

public class FakeLocationService : ILocationService
{
    public async Task<LocationInfo> GetIpInfoAsync(string ip)
    {
        return await Task.FromResult(new LocationInfo
        {
            IP = ip
        });
    }
}
