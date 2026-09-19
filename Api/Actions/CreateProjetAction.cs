using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Shared.Dtos.Projets;

namespace Api.Actions;

/// <summary>
/// Créer un Projet initialise aussi ses 18 Phases (NonCommencee) et calcule le niveau de maturité
/// initial (0, Prompt Maître 10 étape 3) — le tout dans une transaction explicite (section 5.4).
/// </summary>
public class CreateProjetAction(AnalyseProjetDbContext db, MaturiteCalculatorService maturiteCalculator)
{
    private static readonly (int Numero, string Nom)[] NomsPhases =
    [
        (1, "Fiche client"),
        (2, "Découverte du projet"),
        (3, "Processus métier"),
        (4, "Problèmes et besoins"),
        (5, "Acteurs"),
        (6, "Données"),
        (7, "Documents"),
        (8, "Fonctionnalités"),
        (9, "Automatisations"),
        (10, "Contraintes (provisoire)"),
        (11, "Règles métier (provisoire)"),
        (12, "Intégrations (provisoire)"),
        (13, "Non-fonctionnel (provisoire)"),
        (14, "Priorisation MVP (provisoire)"),
        (15, "Planning (provisoire)"),
        (16, "Validation (provisoire)"),
        (17, "Synthèse (provisoire)"),
        (18, "Transfert prompt maître (provisoire)")
    ];

    public async Task<Projet> ExecuterAsync(UpsertProjetDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var projet = new Projet
        {
            Nom = dto.Nom,
            ClientId = dto.ClientId,
            DateCreation = DateTime.UtcNow,
            StackEnvisagee = dto.StackEnvisagee,
            Statut = dto.Statut
        };

        foreach (var (numero, nom) in NomsPhases)
        {
            projet.Phases.Add(new Phase
            {
                Numero = numero,
                Nom = nom,
                Statut = Shared.Enums.StatutPhase.NonCommencee,
                DateMaj = DateTime.UtcNow
            });
        }

        db.Projets.Add(projet);
        await db.SaveChangesAsync(cancellationToken);

        projet.NiveauMaturite = await maturiteCalculator.CalculerAsync(projet.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return projet;
    }
}
