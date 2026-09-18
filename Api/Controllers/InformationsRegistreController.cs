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
[Route("api/projets/{projetId:int}/informations")]
public class InformationsRegistreController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InformationRegistreDto>>> GetToutes(
        int projetId, [FromQuery] int? phaseId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var requete = db.InformationsRegistre.Where(i => i.ProjetId == projetId);
        if (phaseId.HasValue)
        {
            requete = requete.Where(i => i.PhaseId == phaseId);
        }

        var informations = await requete
            .OrderBy(i => i.Id)
            .Select(i => new InformationRegistreDto(
                i.Id, i.Code, i.ProjetId, i.PhaseId, i.Phase != null ? i.Phase.Numero : null,
                i.Libelle, i.Valeur, i.Source, i.Statut))
            .ToListAsync(cancellationToken);

        return Ok(informations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InformationRegistreDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var information = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.Id == id)
            .Select(i => new InformationRegistreDto(
                i.Id, i.Code, i.ProjetId, i.PhaseId, i.Phase != null ? i.Phase.Numero : null,
                i.Libelle, i.Valeur, i.Source, i.Statut))
            .FirstOrDefaultAsync(cancellationToken);

        return information is null ? NotFound() : Ok(information);
    }

    [HttpPost]
    public async Task<ActionResult<InformationRegistreDto>> Creer(
        int projetId, UpsertInformationRegistreDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        if (dto.PhaseId.HasValue && !await db.Phases.AnyAsync(
                p => p.Id == dto.PhaseId && p.ProjetId == projetId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.PhaseId), "La phase indiquée n'existe pas pour ce projet.");
            return ValidationProblem(ModelState);
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "INF", cancellationToken);

        var information = new InformationRegistre
        {
            Code = code,
            ProjetId = projetId,
            PhaseId = dto.PhaseId,
            Libelle = dto.Libelle,
            Valeur = dto.Valeur,
            Source = dto.Source,
            Statut = dto.Statut
        };

        db.InformationsRegistre.Add(information);
        await db.SaveChangesAsync(cancellationToken);

        var numeroPhase = dto.PhaseId.HasValue
            ? await db.Phases.Where(p => p.Id == dto.PhaseId).Select(p => (int?)p.Numero).FirstOrDefaultAsync(cancellationToken)
            : null;

        var resultDto = new InformationRegistreDto(
            information.Id, information.Code, information.ProjetId, information.PhaseId, numeroPhase,
            information.Libelle, information.Valeur, information.Source, information.Statut);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = information.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(
        int projetId, int id, UpsertInformationRegistreDto dto, CancellationToken cancellationToken)
    {
        var information = await db.InformationsRegistre.FirstOrDefaultAsync(
            i => i.ProjetId == projetId && i.Id == id, cancellationToken);
        if (information is null)
        {
            return NotFound();
        }

        if (dto.PhaseId.HasValue && !await db.Phases.AnyAsync(
                p => p.Id == dto.PhaseId && p.ProjetId == projetId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.PhaseId), "La phase indiquée n'existe pas pour ce projet.");
            return ValidationProblem(ModelState);
        }

        information.PhaseId = dto.PhaseId;
        information.Libelle = dto.Libelle;
        information.Valeur = dto.Valeur;
        information.Source = dto.Source;
        information.Statut = dto.Statut;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var information = await db.InformationsRegistre.FirstOrDefaultAsync(
            i => i.ProjetId == projetId && i.Id == id, cancellationToken);
        if (information is null)
        {
            return NotFound();
        }

        db.InformationsRegistre.Remove(information);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
