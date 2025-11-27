using IdentityPrvd.Common.Constants;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Seeding;

public static class IdentityPrvdSeedHelper
{
    public static async Task SeedDefaultsAsync(
        IdentityPrvdContext dbContext,
        IHasher hasher,
        ISessionManagerStore sessionStore,
        string clientAudience = null)
    {
        var itemsCountAdded = 0;
        if (!await dbContext.Roles.AnyAsync())
        {
            await dbContext.Roles.AddRangeAsync(SeedConstants.GetRoles());
            itemsCountAdded++;
        }

        if (!await dbContext.Permissions.AnyAsync())
        {
            await dbContext.Permissions.AddRangeAsync(SeedConstants.GetPermissions());
            itemsCountAdded++;
        }

        if (!await dbContext.Clients.AnyAsync())
        {
            var audience = clientAudience ?? "IdentityPrvd";
            await dbContext.Clients.AddRangeAsync(SeedConstants.GetClients(hasher, audience));
            itemsCountAdded++;
        }

        if (itemsCountAdded > 0)
        {
            await dbContext.SaveChangesAsync();
            await MapRolesAndPermissionsAsync(dbContext);
        }

        await sessionStore.InitializeAsync();
    }

    private static async Task MapRolesAndPermissionsAsync(IdentityPrvdContext dbContext)
    {
        await dbContext.Roles.LoadAsync();
        await dbContext.Permissions.LoadAsync();

        var roles = dbContext.Roles.Local.ToDictionary(r => r.Name, r => r);
        var permissions = dbContext.Permissions.Local.ToDictionary(p => p.Value, p => p);

        var rolePermissions = new List<IdentityRolePermission>();

        if (roles.TryGetValue(DefaultsRoles.Admin, out var adminRole))
        {
            foreach (var permission in permissions.Values)
            {
                rolePermissions.Add(CreateRolePermission(adminRole.Id, permission.Id));
            }
        }

        if (roles.TryGetValue(DefaultsRoles.User, out var userRole))
        {
            var userPermissions = new[]
            {
                IdentityPermissions.Credentials.Manage,
                IdentityPermissions.Sessions.Read,
                IdentityPermissions.Sessions.Manage,
                IdentityPermissions.Mfas.Manage,
                IdentityPermissions.Contacts.Read,
                IdentityPermissions.Contacts.Manage,
                IdentityPermissions.Devices.Read,
                IdentityPermissions.Devices.Manage,
                IdentityPermissions.Qrs.Read,
                IdentityPermissions.Qrs.Manage
            };

            foreach (var permissionValue in userPermissions)
            {
                if (permissions.TryGetValue(permissionValue, out var permission))
                {
                    rolePermissions.Add(CreateRolePermission(userRole.Id, permission.Id));
                }
            }
        }

        if (roles.TryGetValue(DefaultsRoles.Developer, out var developerRole))
        {
            var developerPermissions = new[]
            {
                IdentityPermissions.Clients.Read,
                IdentityPermissions.Roles.Read,
                IdentityPermissions.Permissions.Read
            };

            foreach (var permissionValue in developerPermissions)
            {
                if (permissions.TryGetValue(permissionValue, out var permission))
                {
                    rolePermissions.Add(CreateRolePermission(developerRole.Id, permission.Id));
                }
            }
        }

        if (rolePermissions.Count > 0)
        {
            await dbContext.RolePermissions.AddRangeAsync(rolePermissions);
            await dbContext.SaveChangesAsync();
        }
    }

    private static IdentityRolePermission CreateRolePermission(Guid roleId, Guid permissionId)
    {
        return new IdentityRolePermission
        {
            Id = Guid.CreateVersion7(),
            RoleId = roleId,
            PermissionId = permissionId,
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            IsActive = true
        };
    }
}

