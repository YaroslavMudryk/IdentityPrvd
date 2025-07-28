using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Mappers;

public static class IdentityMapperExtensions
{
    public static AppInfo MapToAppInfo(this IdentityClient client, string appVersion)
    {
        return new AppInfo
        {
            Id = client.Id,
            Name = client.Name,
            Version = appVersion,
            Description = client.Description,
            Image = client.Image,
            ShortName = client.ShortName
        };
    }
}
