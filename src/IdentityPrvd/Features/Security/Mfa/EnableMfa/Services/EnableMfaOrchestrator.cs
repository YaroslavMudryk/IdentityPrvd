using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Security.Mfa.EnableMfa.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace IdentityPrvd.Features.Security.Mfa.EnableMfa.Services;

public class EnableMfaOrchestrator(
    IProtectionService protectionService,
    TimeProvider timeProvider,
    IMfaStore mfaStore,
    IdentityPrvdOptions identityOptions,
    IMfaService mfaService,
    ITransactionManager transactionManager,
    IIdentityContext identityContext)
{
    public async Task<MfaResponse> EnableMfaAsync(MfaDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermission(IdentityPermissions.Mfas.Manage);

        var userId = currentUser.UserId.GetIdAsGuid();
        await using var transaction = await transactionManager.BeginTransactionAsync();

        MfaResponse response = null;
        if (string.IsNullOrWhiteSpace(dto.Totp))
        {
            response = await HandleInitMfaAsync(userId);
        }
        else
        {
            response = await HandleConfirmMfaAsync(dto.Totp, userId, currentUser.SessionId);
        }

        await transaction.CommitAsync();

        return response;
    }

    private async Task<MfaResponse> HandleInitMfaAsync(Guid userId)
    {
        var user = await mfaStore.GetUserAsync(userId);
        var activatedMfa = await mfaStore.GetUserActivatedMfaNullableAsync(userId);
        if (activatedMfa != null)
            throw new BadRequestException("You already have activated mfa");

        var userMfa = await mfaStore.GetUserMfaTotpNullableAsync(userId);
        if (userMfa == null)
        {
            var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(10));
            var mfaId = Guid.CreateVersion7();
            var recoveryCodes = Generator.GetRecoveryCodes();
            var mfaRecoveryCodes = GetMfaRecoveryCodes(mfaId, recoveryCodes);
            var newMfa = new IdentityMfa
            {
                Id = mfaId,
                Activated = null,
                Secret = protectionService.EncryptData(secret),
                UserId = user.Id,
                Type = MfaType.Totp,
                RecoveryCodes = [.. mfaRecoveryCodes]
            };
            await mfaStore.AddAsync(newMfa);
            return new MfaResponse
            {
                RestoreCodes = [.. recoveryCodes],
                SetupCode = secret,
                SetupUrl = new OtpUri(OtpType.Totp, secret, user.Login, identityOptions.App.Name).ToString(),
            };
        }
        else
        {
            var secret = protectionService.DecryptData(userMfa.Secret);
            return new MfaResponse
            {
                RestoreCodes = [],
                SetupCode = secret,
                SetupUrl = new OtpUri(OtpType.Totp, secret, user.Login, identityOptions.App.Name).ToString()
            };
        }
    }

    private async Task<MfaResponse> HandleConfirmMfaAsync(string code, Guid userId, string sessionId)
    {
        var userMfa = await mfaStore.GetUserMfaTotpNullableAsync(userId)
            ?? throw new BadRequestException("Unable activate mfa");

        if (!await mfaService.VerifyMfaAsync(code, userMfa.Secret))
            throw new BadRequestException("Your otp code is invalid");

        userMfa.Activated = timeProvider.GetUtcNow().UtcDateTime;
        userMfa.ActivatedBySessionId = sessionId.GetIdAsGuid();
        await mfaStore.UpdateAsync(userMfa);
        return null;
    }

    private IEnumerable<IdentityMfaRecoveryCode> GetMfaRecoveryCodes(Guid mfaId, IEnumerable<string> codes)
    {
        string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(bytes);
        }

        return codes.Select(code =>
        {
            return new IdentityMfaRecoveryCode
            {
                Id = Guid.CreateVersion7(),
                MfaId = mfaId,
                CodeHash = HashCode(code),
                ExpiryAt = timeProvider.GetUtcNow().UtcDateTime.AddMonths(6), //ToDo: Make it configurable
                IsUsed = false,
                UsedAt = null
            };
        });
    }
}
