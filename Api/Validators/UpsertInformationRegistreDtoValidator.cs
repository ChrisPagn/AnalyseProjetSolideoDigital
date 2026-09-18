using FluentValidation;
using Shared.Dtos.Registres;

namespace Api.Validators;

public class UpsertInformationRegistreDtoValidator : AbstractValidator<UpsertInformationRegistreDto>
{
    public UpsertInformationRegistreDtoValidator()
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Valeur).MaximumLength(2000);
        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x.Statut).IsInEnum();
    }
}
