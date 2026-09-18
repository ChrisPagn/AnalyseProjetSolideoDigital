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
[Route("api/projets/{projetId:int}/acteurs")]
public class ActeursController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
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

        return Ok(acteurs.Select(VersDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActeurDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var acteur = await db.Acteurs
            .Where(a => a.ProjetId == projetId && a.Id == id)
            .Include(a => a.Permissions)
            .FirstOrDefaultAsync(cancellationToken);

        return acteur is null ? NotFound() : Ok(VersDto(acteur));
    }

    [HttpPost]
    public async Task<ActionResult<ActeurDto>> Creer(int projetId, UpsertActeurDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "ACT", cancellationToken);
        var acteur = new Acteur { Code = code, ProjetId = projetId, Nom = dto.Nom, Fonction = dto.Fonction };

        db.Acteurs.Add(acteur);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = acteur.Id }, VersDto(acteur));
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

    private static ActeurDto VersDto(Acteur a) => new(
        a.Id, a.Code, a.ProjetId, a.Nom, a.Fonction, a.Permissions.Select(VersDtoPermission).ToList());

    private static PermissionDto VersDtoPermission(Permission p) => new(
        p.Id, p.ActeurId, p.EntiteConcernee, p.PeutVoir, p.PeutCreer, p.PeutModifier, p.PeutSupprimer, p.PeutValider);
}
