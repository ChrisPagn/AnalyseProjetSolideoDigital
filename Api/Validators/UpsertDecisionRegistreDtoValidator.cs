using FluentValidation;
using Shared.Dtos.Registres;

namespace Api.Validators;

public class UpsertDecisionRegistreDtoValidator : AbstractValidator<UpsertDecisionRegistreDto>
{
    public UpsertDecisionRegistreDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Justification).MaximumLength(1000);
        RuleFor(x => x.AlternativesEcartees).MaximumLength(1000);
    }
}
