using IdentityPrvd.WebApi.Db.Entities.Internal;

namespace IdentityPrvd.WebApi.Features.Signin.Services;

public interface ILocationService
{
    Task<LocationInfo> GetIpInfoAsync(string ip);
}
