using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/fonctionnalites")]
public class FonctionnalitesController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FonctionnaliteDto>>> GetToutes(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var fonctionnalites = await db.Fonctionnalites
            .Where(f => f.ProjetId == projetId)
            .Include(f => f.Acteur)
            .Include(f => f.CriteresAcceptation)
            .Include(f => f.LiensTracabilite)
            .OrderBy(f => f.Id)
            .ToListAsync(cancellationToken);

        return Ok(fonctionnalites.Select(VersDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FonctionnaliteDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var fonctionnalite = await db.Fonctionnalites
            .Where(f => f.ProjetId == projetId && f.Id == id)
            .Include(f => f.Acteur)
            .Include(f => f.CriteresAcceptation)
            .Include(f => f.LiensTracabilite)
            .FirstOrDefaultAsync(cancellationToken);

        return fonctionnalite is null ? NotFound() : Ok(VersDto(fonctionnalite));
    }

    [HttpPost]
    public async Task<ActionResult<FonctionnaliteDto>> Creer(
        int projetId, UpsertFonctionnaliteDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        if (dto.ActeurId.HasValue && !await db.Acteurs.AnyAsync(
                a => a.Id == dto.ActeurId && a.ProjetId == projetId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.ActeurId), "L'acteur indiqué n'existe pas pour ce projet.");
            return ValidationProblem(ModelState);
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "F", cancellationToken);

        var fonctionnalite = new Fonctionnalite
        {
            Code = code,
            ProjetId = projetId,
            ActeurId = dto.ActeurId,
            Nom = dto.Nom,
            Description = dto.Description,
            Priorite = dto.Priorite,
            Statut = dto.Statut
        };

        db.Fonctionnalites.Add(fonctionnalite);

        // La Fonctionnalite doit être persistée avant le recalcul : MaturiteCalculatorService
        // relit les Fonctionnalites par requête SQL directe (voir même bug corrigé sur
        // ProblemesController et à l'étape 4).
        await db.SaveChangesAsync(cancellationToken);

        // Une nouvelle Fonctionnalite est orpheline tant qu'aucun LienTracabilite ne la couvre :
        // peut plafonner NiveauMaturite à 4 (Prompt Maître 4.3).
        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var acteurNom = dto.ActeurId.HasValue
            ? await db.Acteurs.Where(a => a.Id == dto.ActeurId).Select(a => a.Nom).FirstOrDefaultAsync(cancellationToken)
            : null;

        var resultDto = new FonctionnaliteDto(
            fonctionnalite.Id, fonctionnalite.Code, fonctionnalite.ProjetId, fonctionnalite.ActeurId, acteurNom,
            fonctionnalite.Nom, fonctionnalite.Description, fonctionnalite.Priorite, fonctionnalite.Statut,
            EstOrpheline: true, []);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = fonctionnalite.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(
        int projetId, int id, UpsertFonctionnaliteDto dto, CancellationToken cancellationToken)
    {
        var fonctionnalite = await db.Fonctionnalites.FirstOrDefaultAsync(
            f => f.ProjetId == projetId && f.Id == id, cancellationToken);
        if (fonctionnalite is null)
        {
            return NotFound();
        }

        if (dto.ActeurId.HasValue && !await db.Acteurs.AnyAsync(
                a => a.Id == dto.ActeurId && a.ProjetId == projetId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.ActeurId), "L'acteur indiqué n'existe pas pour ce projet.");
            return ValidationProblem(ModelState);
        }

        fonctionnalite.ActeurId = dto.ActeurId;
        fonctionnalite.Nom = dto.Nom;
        fonctionnalite.Description = dto.Description;
        fonctionnalite.Priorite = dto.Priorite;
        fonctionnalite.Statut = dto.Statut;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var fonctionnalite = await db.Fonctionnalites.FirstOrDefaultAsync(
            f => f.ProjetId == projetId && f.Id == id, cancellationToken);
        if (fonctionnalite is null)
        {
            return NotFound();
        }

        db.Fonctionnalites.Remove(fonctionnalite);
        await db.SaveChangesAsync(cancellationToken);

        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // --- Critères d'acceptation ---

    [HttpPost("{fonctionnaliteId:int}/criteres")]
    public async Task<ActionResult<CritereAcceptationDto>> AjouterCritere(
        int projetId, int fonctionnaliteId, UpsertCritereAcceptationDto dto, CancellationToken cancellationToken)
    {
        var fonctionnalite = await db.Fonctionnalites.FirstOrDefaultAsync(
            f => f.ProjetId == projetId && f.Id == fonctionnaliteId, cancellationToken);
        if (fonctionnalite is null)
        {
            return NotFound();
        }

        var critere = new CritereAcceptation
        {
            FonctionnaliteId = fonctionnaliteId,
            Given = dto.Given,
            When = dto.When,
            Then = dto.Then
        };

        db.CriteresAcceptation.Add(critere);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new CritereAcceptationDto(critere.Id, critere.FonctionnaliteId, critere.Given, critere.When, critere.Then));
    }

    [HttpDelete("{fonctionnaliteId:int}/criteres/{critereId:int}")]
    public async Task<IActionResult> SupprimerCritere(
        int projetId, int fonctionnaliteId, int critereId, CancellationToken cancellationToken)
    {
        var critere = await db.CriteresAcceptation
            .Where(c => c.Id == critereId && c.FonctionnaliteId == fonctionnaliteId)
            .Where(c => c.Fonctionnalite!.ProjetId == projetId)
            .FirstOrDefaultAsync(cancellationToken);

        if (critere is null)
        {
            return NotFound();
        }

        db.CriteresAcceptation.Remove(critere);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static FonctionnaliteDto VersDto(Fonctionnalite f) => new(
        f.Id, f.Code, f.ProjetId, f.ActeurId, f.Acteur?.Nom, f.Nom, f.Description, f.Priorite, f.Statut,
        EstOrpheline: f.LiensTracabilite.Count == 0,
        f.CriteresAcceptation.Select(c => new CritereAcceptationDto(c.Id, c.FonctionnaliteId, c.Given, c.When, c.Then)).ToList());
}
