using FluentValidation;
using Shared.Dtos.Clients;

namespace Api.Validators;

public class UpsertClientDtoValidator : AbstractValidator<UpsertClientDto>
{
    public UpsertClientDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Secteur).MaximumLength(200);
        RuleFor(x => x.Taille).MaximumLength(200);
        RuleFor(x => x.Contact).MaximumLength(200);
        RuleFor(x => x.Adresse).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
