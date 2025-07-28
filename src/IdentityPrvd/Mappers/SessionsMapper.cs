using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class SessionsMapper
{
    public static partial IQueryable<SessionDto> ProjectToDto(this IQueryable<IdentitySession> sessions);
}
