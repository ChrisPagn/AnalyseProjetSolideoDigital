using FluentValidation;
using Shared.Dtos.Domaine;

namespace Api.Validators;

public class UpsertDocumentMetierDtoValidator : AbstractValidator<UpsertDocumentMetierDto>
{
    public UpsertDocumentMetierDtoValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Origine).MaximumLength(200);
        RuleFor(x => x.Destination).MaximumLength(200);
        RuleFor(x => x.Format).MaximumLength(50);
        RuleFor(x => x.DureeConservation).MaximumLength(100);
    }
}
