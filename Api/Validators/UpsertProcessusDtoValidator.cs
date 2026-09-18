using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertProcessusDtoValidator : AbstractValidator<UpsertProcessusDto>
{
    public UpsertProcessusDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Declencheur).MaximumLength(500);
    }
}

public class UpsertEtapeProcessusDtoValidator : AbstractValidator<UpsertEtapeProcessusDto>
{
    public UpsertEtapeProcessusDtoValidator()
    {
        RuleFor(x => x.Ordre).GreaterThan(0);
        RuleFor(x => x.Acteur).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Outil).MaximumLength(200);
        RuleFor(x => x.DureeEstimee).MaximumLength(100);
        RuleFor(x => x.ErreursConnues).MaximumLength(500);
        RuleFor(x => x.ExempleDescription).MaximumLength(500);
    }
}
