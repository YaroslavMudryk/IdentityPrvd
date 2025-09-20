using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Services.SystemStatus;

public class DefaultSystemStatus(
    IClientsQuery clientsQuery,
    IClaimsQuery claimsQuery,
    IRolesQuery rolesQuery) : ISystemStatus
{
    public async Task<SystemStatus> GetSystemStatusAsync()
    {
        try
        {
            var existClient = await clientsQuery.IsExistsClientAsync();
            var existRole = await rolesQuery.IsExistsRoleAsync();
            var existClaim = await claimsQuery.IsExistsClaimAsync();

            return (existClient, existRole, existClaim) switch
            {
                (true, true, true) => SystemStatus.ReadyToUse,
                (true, false, false) or (true, true, false) or (false, true, true) or (false, true, false) or (false, false, true) or (true, false, true) => SystemStatus.PartiallyConfigured,
                _ => SystemStatus.NotConfigured
            };
        }
        catch (Exception ex)
        {
            return SystemStatus.NotConfigured;
        }
    }
}
