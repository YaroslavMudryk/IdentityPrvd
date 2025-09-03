namespace IdentityPrvd.Features.Authentication.QrSignin.Dtos;

public class QrCodeDto
{
    public string VerificationId { get; set; }
    public string QrBase64 { get; set; }
}
