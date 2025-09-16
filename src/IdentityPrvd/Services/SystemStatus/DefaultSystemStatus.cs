using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Services.SystemStatus;

public class DefaultSystemStatus(
    IClientsQuery clientsQuery,
    IRolesQuery rolesQuery) : ISystemStatus
{
    public async Task<SystemStatus> GetSystemStatusAsync()
    {
        var existClient = await clientsQuery.IsExistsClientAsync();
        var existRole = await rolesQuery.IsExistsRoleAsync();

        return (existClient, existRole) switch
        {
            (true, true) => SystemStatus.ReadyToUse,
            (true, false) => SystemStatus.PartiallyConfigured,
            (false, true) => SystemStatus.PartiallyConfigured,
            _ => SystemStatus.NotConfigured
        };
    }
}
