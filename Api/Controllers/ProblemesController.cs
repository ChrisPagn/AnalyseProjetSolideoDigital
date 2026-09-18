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
[Route("api/projets/{projetId:int}/problemes")]
public class ProblemesController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProblemeDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        // Tri par ScoreCalcule (decimal) fait côté client, pas en SQL : SQLite ne sait pas
        // traduire un ORDER BY sur une colonne decimal (System.NotSupportedException).
        var problemes = await db.Problemes
            .Where(p => p.ProjetId == projetId)
            .Select(p => new ProblemeDto(
                p.Id, p.Code, p.ProjetId, p.Description, p.Gravite, p.Frequence, p.ImpactTempsHeuresMois,
                p.CoutEstime, p.ScoreCalcule, p.LiensTracabilite.Any()))
            .ToListAsync(cancellationToken);

        return Ok(problemes.OrderByDescending(p => p.ScoreCalcule).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProblemeDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var probleme = await db.Problemes
            .Where(p => p.ProjetId == projetId && p.Id == id)
            .Select(p => new ProblemeDto(
                p.Id, p.Code, p.ProjetId, p.Description, p.Gravite, p.Frequence, p.ImpactTempsHeuresMois,
                p.CoutEstime, p.ScoreCalcule, p.LiensTracabilite.Any()))
            .FirstOrDefaultAsync(cancellationToken);

        return probleme is null ? NotFound() : Ok(probleme);
    }

    [HttpPost]
    public async Task<ActionResult<ProblemeDto>> Creer(int projetId, UpsertProblemeDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "PROB", cancellationToken);

        var probleme = new Probleme
        {
            Code = code,
            ProjetId = projetId,
            Description = dto.Description,
            Gravite = dto.Gravite,
            Frequence = dto.Frequence,
            ImpactTempsHeuresMois = dto.ImpactTempsHeuresMois,
            CoutEstime = dto.CoutEstime,
            ScoreCalcule = dto.Frequence * dto.ImpactTempsHeuresMois
        };

        db.Problemes.Add(probleme);

        // Le Probleme doit être persisté avant le recalcul : MaturiteCalculatorService relit les
        // Problemes par requête SQL directe, qui ne verrait pas une entité restée seulement
        // trackée en mémoire (même bug déjà rencontré et corrigé à l'étape 4).
        await db.SaveChangesAsync(cancellationToken);

        // Un nouveau Probleme non couvert peut plafonner NiveauMaturite à 4 (Prompt Maître 4.3).
        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new ProblemeDto(
            probleme.Id, probleme.Code, probleme.ProjetId, probleme.Description, probleme.Gravite,
            probleme.Frequence, probleme.ImpactTempsHeuresMois, probleme.CoutEstime, probleme.ScoreCalcule, false);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = probleme.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertProblemeDto dto, CancellationToken cancellationToken)
    {
        var probleme = await db.Problemes.FirstOrDefaultAsync(p => p.ProjetId == projetId && p.Id == id, cancellationToken);
        if (probleme is null)
        {
            return NotFound();
        }

        probleme.Description = dto.Description;
        probleme.Gravite = dto.Gravite;
        probleme.Frequence = dto.Frequence;
        probleme.ImpactTempsHeuresMois = dto.ImpactTempsHeuresMois;
        probleme.CoutEstime = dto.CoutEstime;
        probleme.ScoreCalcule = dto.Frequence * dto.ImpactTempsHeuresMois;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var probleme = await db.Problemes.FirstOrDefaultAsync(p => p.ProjetId == projetId && p.Id == id, cancellationToken);
        if (probleme is null)
        {
            return NotFound();
        }

        db.Problemes.Remove(probleme);
        await db.SaveChangesAsync(cancellationToken);

        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
