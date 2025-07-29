using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
