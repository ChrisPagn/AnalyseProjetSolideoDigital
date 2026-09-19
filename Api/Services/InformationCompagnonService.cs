using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Api.Services;

/// <summary>
/// Crée/synchronise l'InformationRegistre compagnon d'une entité du domaine analysé (Acteur,
/// Entite, DocumentMetier, Fonctionnalite, Automatisation) — Option B pour porter Source/Statut
/// sur ces entités, qui n'ont pas ces colonnes elles-mêmes (voir
/// docs/03-proposition-phases-05-18-v2.md). Un seul point d'écriture pour ce mécanisme, réutilisé
/// par tous les controllers concernés plutôt que dupliqué.
/// </summary>
public class InformationCompagnonService(AnalyseProjetDbContext db, CodeSequenceService codeSequence)
{
    /// <summary>
    /// Crée l'InformationRegistre compagnon d'une entité tout juste créée. À appeler après le
    /// SaveChangesAsync qui a persisté l'entité (pour disposer de son Id), dans la même
    /// transaction explicite que l'appelant.
    /// </summary>
    public async Task<InformationRegistre> CreerAsync(
        int projetId, int? phaseId, TypeEntiteDomaine entiteType, int entiteReferenceId, string libelle,
        string? valeur, SourceInformation source, StatutInformation statut, CancellationToken cancellationToken = default)
    {
        var code = await codeSequence.ProchainCodeAsync(projetId, "INF", cancellationToken);

        var information = new InformationRegistre
        {
            Code = code,
            ProjetId = projetId,
            PhaseId = phaseId,
            Libelle = libelle,
            Valeur = valeur,
            Source = source,
            Statut = statut,
            EntiteType = entiteType,
            EntiteReferenceId = entiteReferenceId
        };

        db.InformationsRegistre.Add(information);
        await db.SaveChangesAsync(cancellationToken);

        return information;
    }

    /// <summary>
    /// Met à jour Source/Statut du compagnon existant d'une entité, ou en crée un si aucun
    /// n'existe encore (entité créée avant l'introduction de ce mécanisme, ou créée par un appel
    /// qui n'en fournissait pas).
    /// </summary>
    public async Task ModifierOuCreerAsync(
        int projetId, int? phaseId, TypeEntiteDomaine entiteType, int entiteReferenceId, string libelle,
        string? valeur, SourceInformation source, StatutInformation statut, CancellationToken cancellationToken = default)
    {
        var compagnon = await db.InformationsRegistre.FirstOrDefaultAsync(
            i => i.ProjetId == projetId && i.EntiteType == entiteType && i.EntiteReferenceId == entiteReferenceId,
            cancellationToken);

        if (compagnon is null)
        {
            await CreerAsync(projetId, phaseId, entiteType, entiteReferenceId, libelle, valeur, source, statut, cancellationToken);
            return;
        }

        compagnon.Libelle = libelle;
        compagnon.Valeur = valeur;
        compagnon.Source = source;
        compagnon.Statut = statut;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<InformationRegistre?> ObtenirAsync(
        int projetId, TypeEntiteDomaine entiteType, int entiteReferenceId, CancellationToken cancellationToken = default) =>
        await db.InformationsRegistre.FirstOrDefaultAsync(
            i => i.ProjetId == projetId && i.EntiteType == entiteType && i.EntiteReferenceId == entiteReferenceId,
            cancellationToken);
}
