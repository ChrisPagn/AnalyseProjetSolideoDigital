using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertFonctionnaliteDtoValidator : AbstractValidator<UpsertFonctionnaliteDto>
{
    public UpsertFonctionnaliteDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Priorite).IsInEnum();
        RuleFor(x => x.Statut).IsInEnum();
    }
}

public class UpsertCritereAcceptationDtoValidator : AbstractValidator<UpsertCritereAcceptationDto>
{
    public UpsertCritereAcceptationDtoValidator()
    {
        RuleFor(x => x.Given).NotEmpty().MaximumLength(500);
        RuleFor(x => x.When).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Then).NotEmpty().MaximumLength(500);
    }
}
