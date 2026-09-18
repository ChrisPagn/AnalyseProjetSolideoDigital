using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertActeurDtoValidator : AbstractValidator<UpsertActeurDto>
{
    public UpsertActeurDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Fonction).MaximumLength(200);
    }
}

public class UpsertPermissionDtoValidator : AbstractValidator<UpsertPermissionDto>
{
    public UpsertPermissionDtoValidator()
    {
        RuleFor(x => x.EntiteConcernee).NotEmpty().MaximumLength(200);
    }
}
