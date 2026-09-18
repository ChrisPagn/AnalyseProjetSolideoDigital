using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;

namespace Api.Controllers;

/// <summary>
/// Source de vérité unique pour la couverture besoin ↔ fonctionnalité (Prompt Maître 4.1, 4.3,
/// interdiction 14 : jamais de champ BesoinCouvert en texte libre).
/// </summary>
[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/liens-tracabilite")]
public class LiensTracabiliteController(AnalyseProjetDbContext db, MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LienTracabiliteDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var liens = await db.LiensTracabilite
            .Where(l =>
                (l.Probleme != null && l.Probleme.ProjetId == projetId) ||
                (l.Fonctionnalite != null && l.Fonctionnalite.ProjetId == projetId) ||
                (l.Entite != null && l.Entite.ProjetId == projetId))
            .Include(l => l.Probleme)
            .Include(l => l.Fonctionnalite)
            .Include(l => l.Entite)
            .OrderBy(l => l.Id)
            .Select(l => new LienTracabiliteDto(
                l.Id,
                l.ProblemeId, l.Probleme != null ? l.Probleme.Code : null,
                l.FonctionnaliteId, l.Fonctionnalite != null ? l.Fonctionnalite.Code : null,
                l.EntiteId, l.Entite != null ? l.Entite.Code : null,
                l.CritereAcceptationId))
            .ToListAsync(cancellationToken);

        return Ok(liens);
    }

    [HttpPost]
    public async Task<ActionResult<LienTracabiliteDto>> Creer(
        int projetId, CreerLienTracabiliteDto dto, CancellationToken cancellationToken)
    {
        if (dto is { ProblemeId: null, FonctionnaliteId: null, EntiteId: null, CritereAcceptationId: null })
        {
            ModelState.AddModelError(string.Empty, "Au moins un lien (Problème, Fonctionnalité, Entité ou Critère) est requis.");
            return ValidationProblem(ModelState);
        }

        var lien = new LienTracabilite
        {
            ProblemeId = dto.ProblemeId,
            FonctionnaliteId = dto.FonctionnaliteId,
            EntiteId = dto.EntiteId,
            CritereAcceptationId = dto.CritereAcceptationId
        };

        db.LiensTracabilite.Add(lien);

        // Le lien doit être persisté avant le recalcul : MaturiteCalculatorService relit la
        // couverture (LiensTracabilite.Count == 0) par requête SQL directe (même bug corrigé sur
        // ProblemesController, FonctionnalitesController et à l'étape 4).
        await db.SaveChangesAsync(cancellationToken);

        // Un nouveau lien peut couvrir un Probleme ou une Fonctionnalite jusque-là orphelins,
        // ce qui peut débloquer NiveauMaturite au-delà de 4 (Prompt Maître 4.3).
        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new LienTracabiliteDto(
            lien.Id, lien.ProblemeId, null, lien.FonctionnaliteId, null, lien.EntiteId, null, lien.CritereAcceptationId);

        return CreatedAtAction(nameof(GetTous), new { projetId }, resultDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var lien = await db.LiensTracabilite.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lien is null)
        {
            return NotFound();
        }

        db.LiensTracabilite.Remove(lien);
        await db.SaveChangesAsync(cancellationToken);

        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
