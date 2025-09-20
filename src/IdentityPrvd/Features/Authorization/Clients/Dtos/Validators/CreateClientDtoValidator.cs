using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Clients.Dtos.Validators;

public class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientDtoValidator(IClientsQuery clientsQuery)
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Can't be empty");
        RuleFor(s => s.ClientSecretRequired)
            .NotNull()
            .WithMessage("Can't be null");
        RuleFor(s => s.IsActive)
            .NotNull()
            .WithMessage("Can't be null");
        RuleFor(s => s.ActiveFrom)
            .NotEmpty()
            .WithMessage("Can't be empty");

        RuleFor(s => s.ClientId)
            .MustAsync(async (clientId, token) =>
            {
                var existClientById = await clientsQuery.GetClientByIdAsync(clientId);
                if (existClientById is not null)
                    throw new BadRequestException("Client with the same id already exists");

                return true;
            });
    }
}

public class UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>
{
    public UpdateClientDtoValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Can't be empty");
        RuleFor(s => s.ClientSecretRequired)
            .NotNull()
            .WithMessage("Can't be null");
        RuleFor(s => s.IsActive)
            .NotNull()
            .WithMessage("Can't be null");
        RuleFor(s => s.ActiveFrom)
            .NotEmpty()
            .WithMessage("Can't be empty");
    }
}

public class UpdateClientClaimsDtoValidator : AbstractValidator<UpdateClientClaimsDto>
{
    public UpdateClientClaimsDtoValidator()
    {

    }
}
