using FluentValidation;
using Shared.Dtos.Registres;

namespace Api.Validators;

public class UpsertRisqueRegistreDtoValidator : AbstractValidator<UpsertRisqueRegistreDto>
{
    public UpsertRisqueRegistreDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Probabilite).MaximumLength(100);
        RuleFor(x => x.Impact).MaximumLength(100);
        RuleFor(x => x.Mesure).MaximumLength(500);
    }
}
