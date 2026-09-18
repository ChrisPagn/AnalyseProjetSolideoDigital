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
[Route("api/projets/{projetId:int}/automatisations")]
public class AutomatisationsController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AutomatisationDto>>> GetToutes(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var automatisations = await db.Automatisations
            .Where(a => a.ProjetId == projetId)
            .OrderBy(a => a.Id)
            .Select(a => new AutomatisationDto(a.Id, a.Code, a.ProjetId, a.Declencheur, a.Condition, a.Action, a.ValidationHumaine))
            .ToListAsync(cancellationToken);

        return Ok(automatisations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AutomatisationDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var automatisation = await db.Automatisations
            .Where(a => a.ProjetId == projetId && a.Id == id)
            .Select(a => new AutomatisationDto(a.Id, a.Code, a.ProjetId, a.Declencheur, a.Condition, a.Action, a.ValidationHumaine))
            .FirstOrDefaultAsync(cancellationToken);

        return automatisation is null ? NotFound() : Ok(automatisation);
    }

    [HttpPost]
    public async Task<ActionResult<AutomatisationDto>> Creer(int projetId, UpsertAutomatisationDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "AUTO", cancellationToken);

        var automatisation = new Automatisation
        {
            Code = code,
            ProjetId = projetId,
            Declencheur = dto.Declencheur,
            Condition = dto.Condition,
            Action = dto.Action,
            ValidationHumaine = dto.ValidationHumaine
        };

        db.Automatisations.Add(automatisation);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new AutomatisationDto(automatisation.Id, automatisation.Code, automatisation.ProjetId, automatisation.Declencheur, automatisation.Condition, automatisation.Action, automatisation.ValidationHumaine);
        return CreatedAtAction(nameof(GetParId), new { projetId, id = automatisation.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertAutomatisationDto dto, CancellationToken cancellationToken)
    {
        var automatisation = await db.Automatisations.FirstOrDefaultAsync(a => a.ProjetId == projetId && a.Id == id, cancellationToken);
        if (automatisation is null)
        {
            return NotFound();
        }

        automatisation.Declencheur = dto.Declencheur;
        automatisation.Condition = dto.Condition;
        automatisation.Action = dto.Action;
        automatisation.ValidationHumaine = dto.ValidationHumaine;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var automatisation = await db.Automatisations.FirstOrDefaultAsync(a => a.ProjetId == projetId && a.Id == id, cancellationToken);
        if (automatisation is null)
        {
            return NotFound();
        }

        db.Automatisations.Remove(automatisation);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
