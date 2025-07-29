using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos.Validators;

public class ExternalSigninDtoValidator : AbstractValidator<ExternalSigninDto>
{
    public ExternalSigninDtoValidator(
        IClientsQuery clientsQuery,
        TimeProvider timeProvider)
    {
        RuleFor(s => s.Provider)
            .NotEmpty().WithMessage("Provider is empty");

        RuleFor(s => s.ClientId)
            .NotEmpty().WithMessage("ClientId is empty");

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

                        var verifySecret = true; // verify secret
                        if (!verifySecret)
                            throw new BadRequestException("Secret is invalid");
                    }
                }

                return true;
            });
    }
}
