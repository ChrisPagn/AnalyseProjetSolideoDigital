using FluentValidation;
using Shared.Dtos.Projets;

namespace Api.Validators;

/// <summary>
/// L'existence du ClientId est vérifiée dans ProjetsController (pas ici) : le pipeline de
/// validation automatique FluentValidation.AspNetCore est synchrone et ne peut pas exécuter de
/// règle asynchrone (MustAsync) — une tentative précédente provoquait une 500 sur chaque requête.
/// </summary>
public class UpsertProjetDtoValidator : AbstractValidator<UpsertProjetDto>
{
    public UpsertProjetDtoValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StackEnvisagee).MaximumLength(200);
        RuleFor(x => x.Statut).IsInEnum();
        RuleFor(x => x.ClientId).GreaterThan(0);
    }
}
