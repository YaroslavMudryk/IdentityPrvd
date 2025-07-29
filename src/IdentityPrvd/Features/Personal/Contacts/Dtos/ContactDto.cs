using IdentityPrvd.Domain.Enums;

namespace IdentityPrvd.Features.Personal.Contacts.Dtos;

public class ContactDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Title { get; set; }
    public string Value { get; set; }
    public ContactType Type { get; set; }
    public bool IsMain { get; set; }
    public DateTime UpdatedAt { get; set; }
}
