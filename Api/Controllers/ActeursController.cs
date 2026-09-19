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
[Route("api/projets/{projetId:int}/acteurs")]
public class ActeursController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, InformationCompagnonService informationCompagnon)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActeurDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var acteurs = await db.Acteurs
            .Where(a => a.ProjetId == projetId)
            .Include(a => a.Permissions)
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);

        var compagnons = await ObtenirCompagnonsAsync(projetId, cancellationToken);
        return Ok(acteurs.Select(a => VersDto(a, compagnons)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActeurDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var acteur = await db.Acteurs
            .Where(a => a.ProjetId == projetId && a.Id == id)
            .Include(a => a.Permissions)
            .FirstOrDefaultAsync(cancellationToken);

        if (acteur is null)
        {
            return NotFound();
        }

        var compagnon = await informationCompagnon.ObtenirAsync(projetId, TypeEntiteDomaine.Acteur, id, cancellationToken);
        return Ok(VersDto(acteur, compagnon));
    }

    [HttpPost]
    public async Task<ActionResult<ActeurDto>> Creer(int projetId, UpsertActeurDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var code = await codeSequence.ProchainCodeAsync(projetId, "ACT", cancellationToken);
        var acteur = new Acteur { Code = code, ProjetId = projetId, Nom = dto.Nom, Fonction = dto.Fonction };

        db.Acteurs.Add(acteur);
        await db.SaveChangesAsync(cancellationToken);

        InformationRegistre? compagnon = null;
        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            compagnon = await informationCompagnon.CreerAsync(
                projetId, null, TypeEntiteDomaine.Acteur, acteur.Id, $"Acteur {code} — {dto.Nom}", dto.Fonction,
                dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = acteur.Id }, VersDto(acteur, compagnon));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertActeurDto dto, CancellationToken cancellationToken)
    {
        var acteur = await db.Acteurs.FirstOrDefaultAsync(a => a.ProjetId == projetId && a.Id == id, cancellationToken);
        if (acteur is null)
        {
            return NotFound();
        }

        acteur.Nom = dto.Nom;
        acteur.Fonction = dto.Fonction;
        await db.SaveChangesAsync(cancellationToken);

        if (dto.Source.HasValue && dto.Statut.HasValue)
        {
            await informationCompagnon.ModifierOuCreerAsync(
                projetId, null, TypeEntiteDomaine.Acteur, acteur.Id, $"Acteur {acteur.Code} — {dto.Nom}", dto.Fonction,
                dto.Source.Value, dto.Statut.Value, cancellationToken);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var acteur = await db.Acteurs.FirstOrDefaultAsync(a => a.ProjetId == projetId && a.Id == id, cancellationToken);
        if (acteur is null)
        {
            return NotFound();
        }

        db.Acteurs.Remove(acteur);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // --- Permissions ---

    [HttpPost("{acteurId:int}/permissions")]
    public async Task<ActionResult<PermissionDto>> AjouterPermission(
        int projetId, int acteurId, UpsertPermissionDto dto, CancellationToken cancellationToken)
    {
        var acteur = await db.Acteurs.FirstOrDefaultAsync(a => a.ProjetId == projetId && a.Id == acteurId, cancellationToken);
        if (acteur is null)
        {
            return NotFound();
        }

        var permission = new Permission
        {
            ActeurId = acteurId,
            EntiteConcernee = dto.EntiteConcernee,
            PeutVoir = dto.PeutVoir,
            PeutCreer = dto.PeutCreer,
            PeutModifier = dto.PeutModifier,
            PeutSupprimer = dto.PeutSupprimer,
            PeutValider = dto.PeutValider
        };

        db.Permissions.Add(permission);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(VersDtoPermission(permission));
    }

    [HttpPut("{acteurId:int}/permissions/{permissionId:int}")]
    public async Task<IActionResult> ModifierPermission(
        int projetId, int acteurId, int permissionId, UpsertPermissionDto dto, CancellationToken cancellationToken)
    {
        var permission = await db.Permissions
            .Where(p => p.Id == permissionId && p.ActeurId == acteurId)
            .Where(p => p.Acteur!.ProjetId == projetId)
            .FirstOrDefaultAsync(cancellationToken);

        if (permission is null)
        {
            return NotFound();
        }

        permission.EntiteConcernee = dto.EntiteConcernee;
        permission.PeutVoir = dto.PeutVoir;
        permission.PeutCreer = dto.PeutCreer;
        permission.PeutModifier = dto.PeutModifier;
        permission.PeutSupprimer = dto.PeutSupprimer;
        permission.PeutValider = dto.PeutValider;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{acteurId:int}/permissions/{permissionId:int}")]
    public async Task<IActionResult> SupprimerPermission(
        int projetId, int acteurId, int permissionId, CancellationToken cancellationToken)
    {
        var permission = await db.Permissions
            .Where(p => p.Id == permissionId && p.ActeurId == acteurId)
            .Where(p => p.Acteur!.ProjetId == projetId)
            .FirstOrDefaultAsync(cancellationToken);

        if (permission is null)
        {
            return NotFound();
        }

        db.Permissions.Remove(permission);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<Dictionary<int, InformationRegistre>> ObtenirCompagnonsAsync(int projetId, CancellationToken cancellationToken) =>
        await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.EntiteType == TypeEntiteDomaine.Acteur)
            .ToDictionaryAsync(i => i.EntiteReferenceId!.Value, cancellationToken);

    private static ActeurDto VersDto(Acteur a, Dictionary<int, InformationRegistre> compagnons) =>
        VersDto(a, compagnons.GetValueOrDefault(a.Id));

    private static ActeurDto VersDto(Acteur a, InformationRegistre? compagnon) => new(
        a.Id, a.Code, a.ProjetId, a.Nom, a.Fonction, a.Permissions.Select(VersDtoPermission).ToList(),
        compagnon?.Source, compagnon?.Statut);

    private static PermissionDto VersDtoPermission(Permission p) => new(
        p.Id, p.ActeurId, p.EntiteConcernee, p.PeutVoir, p.PeutCreer, p.PeutModifier, p.PeutSupprimer, p.PeutValider);
}
