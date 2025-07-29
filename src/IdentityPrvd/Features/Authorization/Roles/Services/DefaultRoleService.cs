using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class DefaultRoleService(
    IRoleStore roleStore)
{
    public async Task MakeRoleAsDefaultAsync(Ulid defaultRoleId)
    {
        var allRoles = await roleStore.GetAllRolesAsync();

        foreach (var role in allRoles)
        {
            if (role.Id == defaultRoleId)
                role.IsDefault = true;
            else
                role.IsDefault = false;
        }

        await roleStore.UpdateRangeAsync(allRoles);
    }
}
