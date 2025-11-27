using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Services.SystemStatus;

public class DefaultSystemStatus(
    IClientsQuery clientsQuery,
    IPermissionsQuery permissionsQuery,
    IRolesQuery rolesQuery) : ISystemStatus
{
    public async Task<SystemStatus> GetSystemStatusAsync()
    {
        try
        {
            var existClient = await clientsQuery.IsExistsClientAsync();
            var existRole = await rolesQuery.IsExistsRoleAsync();
            var existPermission = await permissionsQuery.IsExistsPermissionAsync();

            return (existClient, existRole, existPermission) switch
            {
                (true, true, true) => SystemStatus.ReadyToUse,
                (true, false, false) or (true, true, false) or (false, true, true) or (false, true, false) or (false, false, true) or (true, false, true) => SystemStatus.PartiallyConfigured,
                _ => SystemStatus.NotConfigured
            };
        }
        catch (Exception)
        {
            return SystemStatus.NotConfigured;
        }
    }
}
