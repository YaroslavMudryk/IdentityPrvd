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
    TokenOptions tokenOptions,
    IRefreshTokenStore refreshTokenRepo,
    ITokenService tokenService,
    ITransactionManager transactionManager,
    IRefreshTokensQuery refreshTokensQuery)
{
    public async Task<SigninResponseDto> SigninByRefreshTokenAsync(RefreshTokenDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await transactionManager.BeginTransactionAsync();

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
