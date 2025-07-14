using FluentValidation;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.RefreshToken.DataAccess;
using IdentityPrvd.WebApi.Features.RefreshToken.Dtos;
using IdentityPrvd.WebApi.Features.Signin.Dtos;
using IdentityPrvd.WebApi.Features.Signin.Services;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;

namespace IdentityPrvd.WebApi.Features.RefreshToken.Services;

public class RefreshTokenOrchestrator(
    IValidator<RefreshTokenDto> validator,
    TimeProvider timeProvider,
    TokenOptions tokenOptions,
    RefreshTokenRepo refreshTokenRepo,
    ITokenService tokenService,
    IRefreshTokensQuery refreshTokensQuery)
{
    public async Task<SigninResponseDto> SigninByRefreshTokenAsync(RefreshTokenDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await refreshTokenRepo.BeginTransactionAsync();

        var oldRefreshToken = await refreshTokensQuery.GetRefreshTokenWithSessionNullableAsync(dto.Token);
        oldRefreshToken.UsedAt = utcNow;
        await refreshTokenRepo.UpdateAsync(oldRefreshToken);

        var newRefreshToken = new IdentityRefreshToken
        {
            SessionId = oldRefreshToken.SessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = utcNow.AddDays(tokenOptions.RefreshLifeTimeInDays)
        };
        await refreshTokenRepo.AddAsync(newRefreshToken);

        var jwtToken = await tokenService.GetUserTokenAsync(oldRefreshToken.Session.UserId, oldRefreshToken.SessionId.GetIdAsString());
        await transaction.CommitAsync();

        return new SigninResponseDto
        {
            AccessToken = jwtToken.Token,
            RefreshToken = newRefreshToken.Value,
            ExpiredIn = 3600,
        };
    }
}
