using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class ClientsMapper
{
    public static partial IQueryable<ClientDto> ProjectToDto(this IQueryable<IdentityClient> clients);
    public static partial IdentityClient MapToEntity(this CreateClientDto client);
    public static partial ClientDto MapToDto(this IdentityClient client);
}

public static class ClientsMapperExtensions
{
    public static ClientDto MapToDto(this IdentityClient client, string clientSecret)
    {
        var dto = client.MapToDto();
        dto.ClientSecret = clientSecret;
        return dto;
    }
}
