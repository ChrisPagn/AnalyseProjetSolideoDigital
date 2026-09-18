using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertAutomatisationDtoValidator : AbstractValidator<UpsertAutomatisationDto>
{
    public UpsertAutomatisationDtoValidator()
    {
        RuleFor(x => x.Declencheur).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Condition).MaximumLength(500);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(300);
    }
}
