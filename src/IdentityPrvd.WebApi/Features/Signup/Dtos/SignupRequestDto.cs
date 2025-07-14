namespace IdentityPrvd.WebApi.Features.Signup.Dtos;

public class SignupRequestDto
{
    public string UserName { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
