using FluentValidation;
using IdentityPrvd.Common.Exceptions;

namespace IdentityPrvd.Common.Helpers;

public static class ValidationHelper
{
    public static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (validationResult.IsValid)
        {
            return;
        }

        var validationErrors = validationResult.Errors
            .Where(x => x is not null && x.PropertyName is not null)
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray());

        throw new ValidationFailedException(validationErrors);
    }
}
