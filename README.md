# IdentityPrvd

IdentityPrvd is a modular identity and authentication library for ASP.NET Core. It provides endpoints, services, and extensibility for sign-in, sign-up, sessions, MFA, roles/claims, external providers, and more.

## Quick start

1) Install and register in `Program.cs`:

```csharp
using IdentityPrvd.DependencyInjection;
using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.DependencyInjection.Auth.Providers;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityPrvd(builder.Configuration, b =>
{
	b.UseSha512Hasher()
		.UseAesProtectionService()
		.UseIpApiLocationService()
		.UseExternalProviders(p =>
		{
			p.AddGoogle()
			 .AddMicrosoft()
			 .AddGitHub()
			 .AddFacebook();
		})
		.UseRedisSessionManagerStore(b.Options.Connections.Redis)
		.UseDbContext<IdentityPrvdContext>(options => options.UseNpgsql(b.Options.Connections.Db));
});

var app = builder.Build();

app.UseIdentityPrvd();

app.Run();
```

2) Configure `appsettings.json` (section `IdentityPrvd`):

```json
{
  "IdentityPrvd": {
	"Connections": {
	  "Db": "Host=localhost;Database=identity;Username=postgres;Password=postgres",
	  "Redis": "localhost:6379"
	},
	"Token": {
	  "Issuer": "your-app",
	  "Audience": "your-app-clients",
	  "LifeTimeInMinutes": 60,
	  "RefreshLifeTimeInDays": 30,
	  "RefreshTokenExpireWindowInMinutes": 10,
	  "SessionLifeTimeInDays": 7,
	  "SecretKey": "super-secret-key-string"
	},
	"User": {
	  "LoginType": 1,
	  "ConfirmRequired": false,
	  "ConfirmCodeValidInMinutes": 5,
	  "VerifyPasswordOnChangeLogin": false,
	  "UseOldPasswords": true,
	  "ForceSignoutEverywhere": false
	},
	"Password": {
	  "Regex": "",
	  "AllowDigits": true,
	  "MinLength": 8,
	  "MaxLength": 128
	},
	"Protection": {
	  "Key": "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF"
	}
  }
}
```

3) Add Swagger and CORS as needed. Call `app.UseIdentityPrvd()` to wire middleware and map endpoints.

## What gets registered

- Endpoints: discovered via `IEndpoint` and mapped by `app.MapEndpoints()` inside `app.UseIdentityPrvd()`
- Authentication: JWT Bearer + External cookie, configured from `TokenOptions`
- Core features: signup/signin, refresh token, signout, sessions, MFA, roles/claims, contacts, devices, QR signin
- Contexts: `IUserContext`, `ICurrentContext`
- Middlewares: correlation, global exception handler, server-side sessions
- Storage: EF-based stores/queries by default (can be swapped)
- Sessions: server-side session manager + in-memory/Redis store

## Options overview

Everything is rooted at `IdentityPrvdOptions`:

- `Connections`: `Db`, `Redis`
- `Token`: issuer, audience, lifetime, refresh lifetime, session lifetime, `SecretKey`
- `User`: login type, confirmation flow, old passwords, etc.
- `Password`: policy (regex, allow digits, min/max length)
- `Protection`: symmetric key for `AesProtectionService`
- `Language`, `App`
- `Notifiers`, `Sessions`, `Database`, `Location`, `ExternalProviders`

Validate on startup with `IdentityPrvdOptions.ValidateAndThrowIfNeeded()` if needed.

## Swapping services via builder

Use fluent builder extensions (`IdentityPrvd.DependencyInjection.IdentityPrvdBuilderExtensionsCore*`):

- Hasher:
	- `UseSha512Hasher()` (PBKDF2-SHA512)
	- `UseFakeHasher()` for testing
- Protection:
	- `UseAesProtectionService()`
	- `UseFakeProtectionService()`
- Location:
	- `UseIpApiLocationService()`
	- `UseFakeLocationService()`
- Notification:
	- `UseFakeEmailNotifier()`, `UseFakeSmsNotifier()`
	- Or generic: `UseEmailNotifier<TEmailService>()`, `UseSmsNotifier<TSmsService>()`
- Sessions:
	- `UseInMemorySessionManagerStore()`, `UseRedisSessionManagerStore()`
- Transactions/DB:
	- `UseEfTransaction()`
	- `UseDbContext<TContext>(options => ...)`
- Data layer:
	- `UseEfStores()` and `UseEfQueries()` to wire all EF implementations

## Custom implementations

You can replace any service with your own implementation. The builder provides generic hooks:

- Protection service

```csharp
public class MyProtectionService : IProtectionService
{
	public string DecryptData(string cipherText) { /* ... */ }
	public string EncryptData(string plainText) { /* ... */ }
}

services.AddIdentityPrvd(opts => { })
	.UseProtectionService<MyProtectionService>();
```

- Hasher

```csharp
public class BcryptHasher : IHasher
{
	public string GetHash(string content) { /* ... */ }
	public bool Verify(string hashContent, string content) { /* ... */ }
}

builder.Services.AddIdentityPrvd(cfg, b => b.UseHasher<BcryptHasher>());
```

- Location

```csharp
public class MyLocationService : ILocationService
{
	public Task<LocationInfo> GetIpInfoAsync(string ip) { /* ... */ }
}

builder.Services.AddIdentityPrvd(cfg, b => b.UseLocationService<MyLocationService>());
```

- Email/SMS notification

```csharp
public class SendGridEmailService : IEmailService { /* ... */ }
public class TwilioSmsService : ISmsService { /* ... */ }

builder.Services.AddIdentityPrvd(cfg, b =>
{
	b.UseEmailNotifier<SendGridEmailService>()
	 .UseSmsNotifier<TwilioSmsService>();
});
```

## Custom Stores and Queries

Two ways to customize data access:

1) Swap all at once with builder:

```csharp
builder.Services.AddIdentityPrvd(cfg, b =>
{
	b.UseStores<
		MyBanStore,
		MyClaimStore,
		MyClientClaimStore,
		MyClientSecretStore,
		MyClientStore,
		MyConfirmStore,
		MyContactStore,
		MyDeviceStore,
		MyFailedLoginAttemptStore,
		MyMfaRecoveryCodeStore,
		MyMfaStore,
		MyPasswordStore,
		MyQrStore,
		MyRefreshTokenStore,
		MyRoleClaimStore,
		MyRoleStore,
		MySessionStore,
		MyUserLoginStore,
		MyUserRoleStore,
		MyUserStore>()
	 .UseQueries<
		MyBansQuery,
		MyClaimsQuery,
		MyClientClaimsQuery,
		MyClientSecretsQuery,
		MyClientsQuery,
		MyConfirmsQuery,
		MyContactsQuery,
		MyDevicesQuery,
		MyFailedLoginAttemptsQuery,
		MyMfaRecoveryCodesQuery,
		MyMfasQuery,
		MyPasswordsQuery,
		MyQrsQuery,
		MyRefreshTokensQuery,
		MyRoleClaimsQuery,
		MyRolesQuery,
		MySessionsQuery,
		MyUserLoginsQuery,
		MyUserRolesQuery,
		MyUsersQuery>();
});
```

2) Target specific interfaces with `IdentityPrvdOptions.Database` mapping helpers:

```csharp
builder.Services.AddIdentityPrvd(cfg =>
{
	cfg.Database
		.MapUsersQuery<MyUsersQuery>()
		.MapUserStore<MyUserStore>()
		.MapRolesQuery<MyRolesQuery>()
		.MapRoleStore<MyRoleStore>()
		.MapRefreshTokenStore<MyRefreshTokenStore>();
});
```

Interfaces to implement live under `IdentityPrvd.Data.Queries` and `IdentityPrvd.Data.Stores`.

## External providers

Enable providers fluently on the builder (shortcuts) or via `IdentityPrvdOptions.ExternalProviders`:

```csharp
builder.Services.AddIdentityPrvd(cfg, b =>
{
	b.UseExternalProviders(p => p.AddGoogle().AddMicrosoft().AddGitHub());
});
```

Appsettings credentials can be bound to `ExternalProviders` entries (keys match provider names).

## Middleware and endpoints

- `app.UseIdentityPrvd()` wires:
	- `CorrelationContextMiddleware`
	- `GlobalExceptionHandlerMiddleware`
	- `UseAuthentication()` / `UseAuthorization()`
	- server-side sessions
	- endpoint discovery via `IEndpoint` types and `app.MapEndpoints()`

## Sessions

- Manager: `ISessionManager` (default `SessionManager`)
- Store: `ISessionManagerStore` (in-memory or Redis). Choose with builder (`UseInMemorySessionManagerStore` / `UseRedisSessionManagerStore`) or via `IdentityPrvdOptions.Sessions`.

## Security primitives

- Hasher: `IHasher` with `Sha512Hasher` default when opted-in, otherwise `FakeHasher` in default dev setup
- Protection: `IProtectionService` with `AesProtectionService` consuming `Protection.Key` (hex string)

## Minimal end-to-end example

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityPrvd(builder.Configuration, b =>
{
	b.UseSha512Hasher()
	 .UseAesProtectionService()
	 .UseIpApiLocationService()
	 .UseRedisSessionManagerStore(builder.Options.Connections.Redis)
	 .UseDbContext<IdentityPrvdContext>(o => o.UseNpgsql(builder.Options.Connections.Db));
});

var app = builder.Build();
app.UseIdentityPrvd();
app.Run();
```

## Extending further

- Add your own `IEndpoint` implementations to expose custom routes alongside built-ins.
- Replace transactions via `UseTransaction<TTransactionManager>()`.
- Replace only a subset of stores or queries using `IdentityPrvdOptions.Database.Map*` helpers.

## Notes

- Defaults are optimized for development: fake notifiers, fake hasher/protection unless overridden.
- For production enable strong `IHasher`, `IProtectionService`, and Redis-backed sessions.
- Ensure `Token.SecretKey` and `Protection.Key` are strong and stored securely.
