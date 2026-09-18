using FluentValidation;
using Shared.Dtos.Registres;

namespace Api.Validators;

public class UpsertQuestionRegistreDtoValidator : AbstractValidator<UpsertQuestionRegistreDto>
{
    public UpsertQuestionRegistreDtoValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Importance).IsInEnum();
        RuleFor(x => x.Statut).IsInEnum();
    }
}
