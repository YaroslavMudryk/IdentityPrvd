using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Features.Authentication.QrSignin.Dtos;

public class QrRequestDto
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AppVersion { get; set; }
    public string Language { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public ClientInfo Client { get; set; }
}
