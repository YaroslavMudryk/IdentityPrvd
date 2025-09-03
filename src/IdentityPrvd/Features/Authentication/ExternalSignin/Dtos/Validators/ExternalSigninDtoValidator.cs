using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos.Validators;

public class ExternalSigninDtoValidator : AbstractValidator<ExternalSigninDto>
{
    public ExternalSigninDtoValidator(
        IClientsQuery clientsQuery,
        IHasher hasher,
        TimeProvider timeProvider)
    {
        RuleFor(s => s.Provider)
            .NotEmpty().WithMessage("Provider can`t be empty");

        RuleFor(s => s.ClientId)
            .NotEmpty().WithMessage("ClientId can`t be empty");

        RuleFor(s => s.Data)
            .Must((data) =>
            {
                if (data != null)
                {
                    if (!data.All(s => s.Contains(':')))
                    {
                        throw new BadRequestException("Not valid imcoming data. Should be \"fcmToken:u4wijrevg5g2iuk9\"");
                    }

                    if (data.Length > 10)
                    {
                        throw new BadRequestException("Too much items. There should be no more than 10 items");
                    }
                }

                return true;
            });

        RuleFor(s => s)
            .MustAsync(async (dto, token) =>
            {
                if (!string.IsNullOrEmpty(dto.ClientId))
                {
                    var client = await clientsQuery.GetClientByIdAsync(dto.ClientId)
                        ?? throw new NotFoundException($"Client with id:{dto.ClientId} not found");

                    if (!client.IsActive)
                        throw new BadRequestException("Client not activated");

                    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
                    if (client.ActiveFrom > utcNow || client.ActiveTo < utcNow)
                        throw new BadRequestException($"Client out of time ({client.ActiveFrom}:{utcNow}:{client.ActiveTo})");

                    if (client.ClientSecretRequired)
                    {
                        var clientSecret = await clientsQuery.GetClientSecretAsync(dto.ClientId);

                        if (!hasher.Verify(clientSecret.Value, dto.ClientSecret))
                            throw new BadRequestException("Secret is invalid");
                    }
                }

                return true;
            });
    }
}
