using FluentValidation;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Features.Authentication.Signup.Dtos;

namespace IdentityPrvd.Features.Authentication.Signup.Services;

public class SignupConfirmOrchestrator(
    IConfirmStore confirmStore,
    IUserStore userStore,
    ITransactionManager transactionManager,
    IValidator<SignupConfirmRequestDto> validator,
    TimeProvider timeProvider)
{
    public async Task ConfirmAsync(SignupConfirmRequestDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var confirm = await confirmStore.GetConfirmByCodeAsync(dto.Code);
        confirm.IsActivated = true;
        confirm.ActivatedAt = utcNow;
        await confirmStore.UpdateAsync(confirm);
        var user = await userStore.GetUserAsync(confirm.UserId);
        user.IsConfirmed = true;
        user.ConfirmedAt = utcNow;
        user.ConfirmedBy = user.Id.GetIdAsString();
        await userStore.UpdateAsync(user);
        await transaction.CommitAsync();
    }
}
