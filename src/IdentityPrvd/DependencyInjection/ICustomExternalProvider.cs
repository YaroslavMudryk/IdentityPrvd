using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.DependencyInjection;

public interface ICustomExternalProvider
{
	void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions);
}


