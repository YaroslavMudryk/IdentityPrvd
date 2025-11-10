using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Security.Sessions.GetSession.Dtos;
using IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class SessionsMapper
{
    public static partial SessionDetailDto MapToDto(this IdentitySession session);
}

public static partial class SessionsMapperExtensions
{
    public static SessionDto MapDto(this IdentitySession dbSession)
    {
        return new SessionDto
        {
            Id = dbSession.Id,
            App = $"{dbSession.App.Name} {dbSession.App.Version}",
            Image = dbSession.App.Image,
            Client = $"{dbSession.Client}",
            CreatedAt = dbSession.CreatedAt,
            Location = $"{dbSession.Location.Country}, {dbSession.Location.City} ({dbSession.Location.Region})",
            Status = dbSession.Status
        };
    }
}
