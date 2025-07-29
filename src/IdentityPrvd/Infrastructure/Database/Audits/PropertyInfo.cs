namespace IdentityPrvd.Infrastructure.Database.Audits;

public class PropertyInfo(string name, string oldValue, string newValue)
{
    public string Name { get; set; } = name;
    public string OldValue { get; set; } = oldValue;
    public string NewValue { get; set; } = newValue;

    public bool IsChanged => OldValue != NewValue;
}
