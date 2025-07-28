using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Options;

namespace IdentityPrvd.Features.Security.RefreshToken.Dtos.Validators;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator(
        IRefreshTokensQuery refreshTokenQuery,
        TokenOptions tokenOptions,
        TimeProvider timeProvider)
    {
        RuleFor(s => s).MustAsync(async (dto, _) =>
        {
            var refreshTokenEntity = await refreshTokenQuery.GetRefreshTokenWithSessionNullableAsync(dto.Token) ?? throw new NotFoundException($"Refresh token ({dto.Token}) not found");

            if (refreshTokenEntity.Session.Status == SessionStatus.Close)
                throw new BadRequestException($"Refresh token ({dto.Token}) related to closed session and can't be activated");

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            //IsExpiredRefreshToken(refreshTokenEntity, utcNow, tokenOptions);

            if (refreshTokenEntity.ExpiredAt < utcNow)
                throw new BadRequestException($"Refresh token ({dto.Token}) already expired");
            if (refreshTokenEntity.UsedAt != null)
                throw new BadRequestException($"Refresh token ({dto.Token}) already used at {refreshTokenEntity.UsedAt}");

            return true;
        });
    }

    //private static bool IsExpiredRefreshToken(RefreshToken refreshTokenEntity, DateTime utcNow, Shared.Options.TokenOptions tokenOptions)
    //{
    //    if (refreshTokenEntity.ExpiredAt > utcNow)
    //    {
    //        if (refreshTokenEntity.TokenUsedAt.HasValue)
    //        {
    //            var maxTimeWithWindow = utcNow.AddMinutes(tokenOptions.RefreshTokenExpireWindowInMinutes);
    //            if (maxTimeWithWindow - refreshTokenEntity.TokenUsedAt.Value == TimeSpan.FromMinutes(tokenOptions.RefreshTokenExpireWindowInMinutes))
    //            {
    //                throw new BadRequestException($"Refresh token already expired");
    //            }
    //        }
    //    }

    //    return true;
    //}
}
