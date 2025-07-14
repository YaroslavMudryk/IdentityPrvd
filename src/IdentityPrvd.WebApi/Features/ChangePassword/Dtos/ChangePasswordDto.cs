namespace IdentityPrvd.WebApi.Features.ChangePassword.Dtos;

public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string Hint { get; set; }
    public string NewPassword { get; set; }
    public bool SignoutEverywhere { get; set; }
}
