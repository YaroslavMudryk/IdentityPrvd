using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IdentityPrvd.WebApi.Db.Audits;

public static class EntityParser
{
    public static List<PropertyInfo> ParseChanges(EntityEntry entityEntry)
    {
        ArgumentNullException.ThrowIfNull(entityEntry);

        var props = entityEntry.Properties;

        if (entityEntry.State != EntityState.Added)
            props = props.Where(s => s.IsModified);

        return [.. props.Select(trackProperty =>
        {
            var propName = trackProperty.Metadata.Name;
            var oldValue = GetOldValue(trackProperty, entityEntry.State);
            var newValue = GetNewValue(trackProperty, entityEntry.State);
            return new PropertyInfo(propName, oldValue, newValue);
        }).Where(s => s.IsChanged)];
    }

    private static string GetOldValue(PropertyEntry trackProperty, EntityState state)
    {
        return state == EntityState.Modified || state == EntityState.Deleted
            ? trackProperty.OriginalValue != null ? trackProperty.OriginalValue.ToString()! : string.Empty : string.Empty;
    }

    private static string GetNewValue(PropertyEntry trackProperty, EntityState state)
    {
        return trackProperty.CurrentValue != null ? trackProperty.CurrentValue.ToString()! : string.Empty;
    }
}
