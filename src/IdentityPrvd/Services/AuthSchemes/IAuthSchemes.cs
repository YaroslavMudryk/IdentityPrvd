namespace IdentityPrvd.Services.AuthSchemes;

public interface IAuthSchemes
{
    Task<List<AuthSchemeDto>> GetAllSchemesAsync();
    Task<List<AuthSchemeDto>> GetAvailableSchemesAsync();
}
