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
[Route("api/projets/{projetId:int}/risques")]
public class RisquesRegistreController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RisqueRegistreDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var risques = await db.RisquesRegistre
            .Where(r => r.ProjetId == projetId)
            .OrderBy(r => r.Id)
            .Select(r => new RisqueRegistreDto(r.Id, r.Code, r.ProjetId, r.Description, r.Probabilite, r.Impact, r.Mesure))
            .ToListAsync(cancellationToken);

        return Ok(risques);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RisqueRegistreDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var risque = await db.RisquesRegistre
            .Where(r => r.ProjetId == projetId && r.Id == id)
            .Select(r => new RisqueRegistreDto(r.Id, r.Code, r.ProjetId, r.Description, r.Probabilite, r.Impact, r.Mesure))
            .FirstOrDefaultAsync(cancellationToken);

        return risque is null ? NotFound() : Ok(risque);
    }

    [HttpPost]
    public async Task<ActionResult<RisqueRegistreDto>> Creer(
        int projetId, UpsertRisqueRegistreDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "R", cancellationToken);

        var risque = new RisqueRegistre
        {
            Code = code,
            ProjetId = projetId,
            Description = dto.Description,
            Probabilite = dto.Probabilite,
            Impact = dto.Impact,
            Mesure = dto.Mesure
        };

        db.RisquesRegistre.Add(risque);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new RisqueRegistreDto(risque.Id, risque.Code, risque.ProjetId, risque.Description, risque.Probabilite, risque.Impact, risque.Mesure);
        return CreatedAtAction(nameof(GetParId), new { projetId, id = risque.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertRisqueRegistreDto dto, CancellationToken cancellationToken)
    {
        var risque = await db.RisquesRegistre.FirstOrDefaultAsync(r => r.ProjetId == projetId && r.Id == id, cancellationToken);
        if (risque is null)
        {
            return NotFound();
        }

        risque.Description = dto.Description;
        risque.Probabilite = dto.Probabilite;
        risque.Impact = dto.Impact;
        risque.Mesure = dto.Mesure;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var risque = await db.RisquesRegistre.FirstOrDefaultAsync(r => r.ProjetId == projetId && r.Id == id, cancellationToken);
        if (risque is null)
        {
            return NotFound();
        }

        db.RisquesRegistre.Remove(risque);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
