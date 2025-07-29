using IdentityPrvd.Domain.Enums;

namespace IdentityPrvd.Features.Personal.Contacts.Dtos;

public class CreateContactDto
{
    public string Title { get; set; }
    public string Value { get; set; }
    public ContactType Type { get; set; }
}
