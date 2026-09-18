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
[Route("api/projets/{projetId:int}/documents")]
public class DocumentsMetierController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentMetierDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var documents = await db.DocumentsMetier
            .Where(d => d.ProjetId == projetId)
            .OrderBy(d => d.Id)
            .Select(d => new DocumentMetierDto(d.Id, d.Code, d.ProjetId, d.Type, d.Origine, d.Destination, d.Format, d.DureeConservation))
            .ToListAsync(cancellationToken);

        return Ok(documents);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentMetierDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var document = await db.DocumentsMetier
            .Where(d => d.ProjetId == projetId && d.Id == id)
            .Select(d => new DocumentMetierDto(d.Id, d.Code, d.ProjetId, d.Type, d.Origine, d.Destination, d.Format, d.DureeConservation))
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentMetierDto>> Creer(int projetId, UpsertDocumentMetierDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "DOC", cancellationToken);

        var document = new DocumentMetier
        {
            Code = code,
            ProjetId = projetId,
            Type = dto.Type,
            Origine = dto.Origine,
            Destination = dto.Destination,
            Format = dto.Format,
            DureeConservation = dto.DureeConservation
        };

        db.DocumentsMetier.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        var resultDto = new DocumentMetierDto(document.Id, document.Code, document.ProjetId, document.Type, document.Origine, document.Destination, document.Format, document.DureeConservation);
        return CreatedAtAction(nameof(GetParId), new { projetId, id = document.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertDocumentMetierDto dto, CancellationToken cancellationToken)
    {
        var document = await db.DocumentsMetier.FirstOrDefaultAsync(d => d.ProjetId == projetId && d.Id == id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        document.Type = dto.Type;
        document.Origine = dto.Origine;
        document.Destination = dto.Destination;
        document.Format = dto.Format;
        document.DureeConservation = dto.DureeConservation;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var document = await db.DocumentsMetier.FirstOrDefaultAsync(d => d.ProjetId == projetId && d.Id == id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        db.DocumentsMetier.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
