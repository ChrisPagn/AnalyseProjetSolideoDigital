using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertProblemeDtoValidator : AbstractValidator<UpsertProblemeDto>
{
    public UpsertProblemeDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Gravite).IsInEnum();
        RuleFor(x => x.Frequence).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImpactTempsHeuresMois).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CoutEstime).GreaterThanOrEqualTo(0).When(x => x.CoutEstime.HasValue);
    }
}
