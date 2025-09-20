namespace IdentityPrvd.Features.Authorization.Clients.Dtos;

public class UpdateClientDto : CreateClientDto
{
    public Ulid Id { get; set; }
}
