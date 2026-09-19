using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;
using Shared.Enums;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/documents")]
public class DocumentsMetierController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, InformationCompagnonService informationCompagnon)
    : ControllerBase
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
            .ToListAsync(cancellationToken);

        var compagnons = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.EntiteType == TypeEntiteDomaine.DocumentMetier)
            .ToDictionaryAsync(i => i.EntiteReferenceId!.Value, cancellationToken);

        return Ok(documents.Select(d => VersDto(d, compagnons.GetValueOrDefault(d.Id))).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentMetierDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var document = await db.DocumentsMetier.FirstOrDefaultAsync(d => d.ProjetId == projetId && d.Id == id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var compagnon = await informationCompagnon.ObtenirAsync(projetId, TypeEntiteDomaine.DocumentMetier, id, cancellationToken);
        return Ok(VersDto(document, compagnon));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentMetierDto>> Creer(int projetId, UpsertDocumentMetierDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

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

        InformationRegistre? compagnon = null;
        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            compagnon = await informationCompagnon.CreerAsync(
                projetId, null, TypeEntiteDomaine.DocumentMetier, document.Id, $"Document {code} — {dto.Type}",
                dto.Origine, dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = document.Id }, VersDto(document, compagnon));
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

        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            await informationCompagnon.ModifierOuCreerAsync(
                projetId, null, TypeEntiteDomaine.DocumentMetier, document.Id, $"Document {document.Code} — {dto.Type}",
                dto.Origine, dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

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

    private static DocumentMetierDto VersDto(DocumentMetier d, InformationRegistre? compagnon) => new(
        d.Id, d.Code, d.ProjetId, d.Type, d.Origine, d.Destination, d.Format, d.DureeConservation,
        compagnon?.Source, compagnon?.Statut);
}
