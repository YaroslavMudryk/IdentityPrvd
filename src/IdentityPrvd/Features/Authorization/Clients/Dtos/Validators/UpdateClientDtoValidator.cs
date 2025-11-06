using FluentValidation;

namespace IdentityPrvd.Features.Authorization.Clients.Dtos.Validators;

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
