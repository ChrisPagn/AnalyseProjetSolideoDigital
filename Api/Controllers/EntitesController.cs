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
[Route("api/projets/{projetId:int}/entites")]
public class EntitesController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, InformationCompagnonService informationCompagnon)
    : ControllerBase
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
            .ToListAsync(cancellationToken);

        var compagnons = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.EntiteType == TypeEntiteDomaine.Entite)
            .ToDictionaryAsync(i => i.EntiteReferenceId!.Value, cancellationToken);

        return Ok(entites.Select(e => VersDto(e, compagnons.GetValueOrDefault(e.Id))).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntiteDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var entite = await db.Entites.FirstOrDefaultAsync(e => e.ProjetId == projetId && e.Id == id, cancellationToken);
        if (entite is null)
        {
            return NotFound();
        }

        var compagnon = await informationCompagnon.ObtenirAsync(projetId, TypeEntiteDomaine.Entite, id, cancellationToken);
        return Ok(VersDto(entite, compagnon));
    }

    [HttpPost]
    public async Task<ActionResult<EntiteDto>> Creer(int projetId, UpsertEntiteDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

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

        InformationRegistre? compagnon = null;
        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            compagnon = await informationCompagnon.CreerAsync(
                projetId, null, TypeEntiteDomaine.Entite, entite.Id, $"Entité {code} — {dto.Nom}", dto.Description,
                dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = entite.Id }, VersDto(entite, compagnon));
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

        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            await informationCompagnon.ModifierOuCreerAsync(
                projetId, null, TypeEntiteDomaine.Entite, entite.Id, $"Entité {entite.Code} — {dto.Nom}", dto.Description,
                dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

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

    private static EntiteDto VersDto(Entite e, InformationRegistre? compagnon) => new(
        e.Id, e.Code, e.ProjetId, e.Nom, e.Description, e.Attributs, e.Relations, compagnon?.Source, compagnon?.Statut);
}
