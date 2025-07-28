using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Domain.ValueObjects;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Services;

public static class ExternalSigninEntityFactory
{
    public static IdentityUser CreateIdentityUser(ExternalUserDto user, DateTime confirmedAt, string confirmedBy)
    {
        return new IdentityUser
        {
            Id = Ulid.NewUlid(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Login = user.Email,
            UserName = Generator.GetUsername(),
            CanBeBlocked = true,
            ConfirmedAt = confirmedAt,
            ConfirmedBy = confirmedBy,
            IsConfirmed = true,
            Image = user.Picture,
            PasswordHash = null
        };
    }

    public static IdentityUserRole CreateIdentityUserRole(Ulid userId, Ulid roleId)
    {
        return new IdentityUserRole
        {
            Id = Ulid.NewUlid(),
            UserId = userId,
            RoleId = roleId
        };
    }

    public static IdentityUserLogin CreateIdentityUserLogin(Ulid userId, string provider, string providerUserId)
    {
        return new IdentityUserLogin
        {
            Id = Ulid.NewUlid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId
        };
    }

    public static IdentityRefreshToken CreateRefreshToken(Ulid sessionId, string value, DateTime expiredAt)
    {
        return new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = value,
            ExpiredAt = expiredAt
        };
    }

    public static IdentitySession CreateIdentitySession(
        Ulid sessionId, 
        Ulid userId, 
        ExternalSigninDto dto, 
        IdentityClient client, 
        LocationInfo location, 
        IdentityRefreshToken refreshToken,
        DateTime expireAt)
    {
        return new IdentitySession
        {
            Id = sessionId,
            UserId = userId,
            Client = dto.MapToClientInfo(),
            App = client.MapToAppInfo(dto.AppVersion),
            Location = location,
            Language = dto.Language,
            Status = SessionStatus.Active,
            Type = SessionType.ExternalProvider,
            ExpireAt = expireAt,
            ViaMFA = false,
            Tokens = [refreshToken]
        };
    }
}
