using IdentityPrvd.Domain.ValueObjects;
using Riok.Mapperly.Abstractions;
using Ext = Extensions.DeviceDetector.Models.ClientInfo;

namespace IdentityPrvd.Mappers;


[Mapper]
public static partial class DefaultMapper
{
    [MapProperty(nameof(Ext.OS), nameof(ClientInfo.Os))]
    public static partial ClientInfo MapToClientInfo(this Ext clientInfo);
}
