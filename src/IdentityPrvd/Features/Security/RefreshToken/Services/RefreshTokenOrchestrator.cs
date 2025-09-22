using FluentValidation;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Security.RefreshToken.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Security.RefreshToken.Services;

public class RefreshTokenOrchestrator(
    IValidator<RefreshTokenDto> validator,
    TimeProvider timeProvider,
    IdentityPrvdOptions identityOptions,
    IRefreshTokenStore refreshTokenStore,
    ITokenService tokenService,
    ITransactionManager transactionManager)
{
    public async Task<SigninResponseDto> SigninByRefreshTokenAsync(RefreshTokenDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var oldRefreshToken = await refreshTokenStore.GetRefreshTokenWithSessionByValueAsync(dto.Token);
        oldRefreshToken.UsedAt = utcNow;
        await refreshTokenStore.UpdateAsync(oldRefreshToken);

        var newRefreshToken = new IdentityRefreshToken
        {
            SessionId = oldRefreshToken.SessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = utcNow.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };
        await refreshTokenStore.AddAsync(newRefreshToken);

        var jwtToken = await tokenService.GetUserTokenAsync(oldRefreshToken.Session.UserId, oldRefreshToken.SessionId.GetIdAsString());
        await transaction.CommitAsync();

        return new SigninResponseDto
        {
            AccessToken = jwtToken.Token,
            RefreshToken = newRefreshToken.Value,
            ExpireIn = identityOptions.Token.LifeTimeInMinutes * 60,
        };
    }
}
