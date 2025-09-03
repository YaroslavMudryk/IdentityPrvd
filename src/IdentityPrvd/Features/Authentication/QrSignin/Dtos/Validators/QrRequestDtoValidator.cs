using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authentication.QrSignin.Dtos.Validators;

public class QrRequestDtoValidator : AbstractValidator<QrRequestDto>
{
    public QrRequestDtoValidator(
        TimeProvider timeProvider,
        IClientsQuery clientsQuery)
    {
        RuleFor(x => x).MustAsync(async (dto, _) =>
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId)
                    ?? throw new NotFoundException($"Client {dto.ClientId} not found");

            if (!client.IsActive)
                throw new BadRequestException("Client is not active");

            if (client.ActiveFrom > utcNow || client.ActiveTo.HasValue && client.ActiveTo.Value < utcNow)
                throw new BadRequestException("Client is not active at this time");

            if (client.ClientSecretRequired)
            {
                var secret = await clientsQuery.GetClientSecretNullableAsync(dto.ClientId)
                    ?? throw new NotFoundException($"Not found secret for clientId:{dto.ClientId}");

                var verifySecret = true; // verify secret
                if (!verifySecret)
                    throw new BadRequestException("Secret is invalid");
            }

            return true;
        });
    }
}
