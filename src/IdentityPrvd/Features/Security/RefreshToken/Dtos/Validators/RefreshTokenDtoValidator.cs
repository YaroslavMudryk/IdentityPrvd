using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Options;

namespace IdentityPrvd.Features.Security.RefreshToken.Dtos.Validators;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator(
        IRefreshTokensQuery refreshTokenQuery,
        IdentityPrvdOptions identityOptions,
        TimeProvider timeProvider)
    {
        RuleFor(s => s).MustAsync(async (dto, _) =>
        {
            var refreshTokenEntity = await refreshTokenQuery.GetRefreshTokenWithSessionNullableAsync(dto.Token) ?? throw new NotFoundException($"Refresh token ({dto.Token}) not found");

            if (refreshTokenEntity.Session.Status == SessionStatus.Close)
                throw new BadRequestException($"Refresh token ({dto.Token}) related to closed session and can't be activated");

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            //IsExpiredRefreshToken(refreshTokenEntity, utcNow, identityOptions);

            if (refreshTokenEntity.ExpiredAt < utcNow)
                throw new BadRequestException($"Refresh token ({dto.Token}) already expired");
            if (refreshTokenEntity.UsedAt != null)
                throw new BadRequestException($"Refresh token ({dto.Token}) already used at {refreshTokenEntity.UsedAt}");

            return true;
        });
    }

    //private static bool IsExpiredRefreshToken(IdentityRefreshToken refreshTokenEntity, DateTime utcNow, IdentityPrvdOptions identityOptions)
    //{
    //    if (refreshTokenEntity.ExpiredAt > utcNow)
    //    {
    //        if (refreshTokenEntity.UsedAt.HasValue)
    //        {
    //            var maxTimeWithWindow = utcNow.AddMinutes(identityOptions.Token.RefreshTokenExpireWindowInMinutes);
    //            if (maxTimeWithWindow - refreshTokenEntity.UsedAt.Value == TimeSpan.FromMinutes(identityOptions.Token.RefreshTokenExpireWindowInMinutes))
    //            {
    //                throw new BadRequestException($"Refresh token already expired");
    //            }
    //        }
    //    }

    //    return true;
    //}
}
