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
[Route("api/projets/{projetId:int}/entites")]
public class EntitesController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EntiteDto>>> GetToutes(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var entites = await db.Entites
            .Where(e => e.ProjetId == projetId)
            .OrderBy(e => e.Id)
            .Select(e => new EntiteDto(e.Id, e.Code, e.ProjetId, e.Nom, e.Description, e.Attributs, e.Relations))
            .ToListAsync(cancellationToken);

        return Ok(entites);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntiteDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var entite = await db.Entites
            .Where(e => e.ProjetId == projetId && e.Id == id)
            .Select(e => new EntiteDto(e.Id, e.Code, e.ProjetId, e.Nom, e.Description, e.Attributs, e.Relations))
            .FirstOrDefaultAsync(cancellationToken);

        return entite is null ? NotFound() : Ok(entite);
    }

    [HttpPost]
    public async Task<ActionResult<EntiteDto>> Creer(int projetId, UpsertEntiteDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "ENT", cancellationToken);

        var entite = new Entite
        {
            Code = code,
            ProjetId = projetId,
            Nom = dto.Nom,
            Description = dto.Description,
            Attributs = dto.Attributs,
            Relations = dto.Relations
        };

        db.Entites.Add(entite);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new EntiteDto(entite.Id, entite.Code, entite.ProjetId, entite.Nom, entite.Description, entite.Attributs, entite.Relations);
        return CreatedAtAction(nameof(GetParId), new { projetId, id = entite.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertEntiteDto dto, CancellationToken cancellationToken)
    {
        var entite = await db.Entites.FirstOrDefaultAsync(e => e.ProjetId == projetId && e.Id == id, cancellationToken);
        if (entite is null)
        {
            return NotFound();
        }

        entite.Nom = dto.Nom;
        entite.Description = dto.Description;
        entite.Attributs = dto.Attributs;
        entite.Relations = dto.Relations;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var entite = await db.Entites.FirstOrDefaultAsync(e => e.ProjetId == projetId && e.Id == id, cancellationToken);
        if (entite is null)
        {
            return NotFound();
        }

        db.Entites.Remove(entite);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
