using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Registres;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/decisions")]
public class DecisionsRegistreController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DecisionRegistreDto>>> GetToutes(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var decisions = await db.DecisionsRegistre
            .Where(d => d.ProjetId == projetId)
            .OrderBy(d => d.Id)
            .Select(d => new DecisionRegistreDto(d.Id, d.Code, d.ProjetId, d.Description, d.Justification, d.AlternativesEcartees, d.Date))
            .ToListAsync(cancellationToken);

        return Ok(decisions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DecisionRegistreDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var decision = await db.DecisionsRegistre
            .Where(d => d.ProjetId == projetId && d.Id == id)
            .Select(d => new DecisionRegistreDto(d.Id, d.Code, d.ProjetId, d.Description, d.Justification, d.AlternativesEcartees, d.Date))
            .FirstOrDefaultAsync(cancellationToken);

        return decision is null ? NotFound() : Ok(decision);
    }

    [HttpPost]
    public async Task<ActionResult<DecisionRegistreDto>> Creer(
        int projetId, UpsertDecisionRegistreDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "DEC", cancellationToken);

        var decision = new DecisionRegistre
        {
            Code = code,
            ProjetId = projetId,
            Description = dto.Description,
            Justification = dto.Justification,
            AlternativesEcartees = dto.AlternativesEcartees,
            Date = DateTime.UtcNow
        };

        db.DecisionsRegistre.Add(decision);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new DecisionRegistreDto(
            decision.Id, decision.Code, decision.ProjetId, decision.Description, decision.Justification, decision.AlternativesEcartees, decision.Date);
        return CreatedAtAction(nameof(GetParId), new { projetId, id = decision.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertDecisionRegistreDto dto, CancellationToken cancellationToken)
    {
        var decision = await db.DecisionsRegistre.FirstOrDefaultAsync(d => d.ProjetId == projetId && d.Id == id, cancellationToken);
        if (decision is null)
        {
            return NotFound();
        }

        decision.Description = dto.Description;
        decision.Justification = dto.Justification;
        decision.AlternativesEcartees = dto.AlternativesEcartees;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var decision = await db.DecisionsRegistre.FirstOrDefaultAsync(d => d.ProjetId == projetId && d.Id == id, cancellationToken);
        if (decision is null)
        {
            return NotFound();
        }

        db.DecisionsRegistre.Remove(decision);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
