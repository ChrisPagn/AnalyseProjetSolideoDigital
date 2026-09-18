using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertEntiteDtoValidator : AbstractValidator<UpsertEntiteDto>
{
    public UpsertEntiteDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Attributs).MaximumLength(2000);
        RuleFor(x => x.Relations).MaximumLength(1000);
    }
}
