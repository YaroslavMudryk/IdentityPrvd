using System.Text;

namespace IdentityPrvd.Domain.ValueObjects;

public class ClientInfo
{
    public DeviceInfo Device { get; set; }
    public OsInfo Os { get; set; }
    public BrowserInfo Browser { get; set; }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        if (Browser == null)
        {
            stringBuilder.Append(Device.Brand);
            stringBuilder.Append(' ');
            stringBuilder.Append(Device.Model);
            if (Os != null)
            {
                stringBuilder.Append(" (");
                stringBuilder.Append(Os.Name);
                stringBuilder.Append(' ');
                stringBuilder.Append(Os.Version);
                stringBuilder.Append(')');
            }
        }
        else
        {
            stringBuilder.Append(Browser.Name);
            stringBuilder.Append(' ');
            stringBuilder.Append(Browser.Version);
            if (Os != null)
            {
                stringBuilder.Append(" (");
                stringBuilder.Append(Os.Name);
                stringBuilder.Append(' ');
                stringBuilder.Append(Os.Version);
                stringBuilder.Append(')');
            }
        }

        return stringBuilder.ToString();
    }
}
