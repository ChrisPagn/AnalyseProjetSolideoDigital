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
[Route("api/projets/{projetId:int}/processus")]
public class ProcessusController(AnalyseProjetDbContext db, CodeSequenceService codeSequence) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProcessusDto>>> GetTous(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var processus = await db.Processus
            .Where(p => p.ProjetId == projetId)
            .Include(p => p.Etapes)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return Ok(processus.Select(VersDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessusDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var processus = await db.Processus
            .Where(p => p.ProjetId == projetId && p.Id == id)
            .Include(p => p.Etapes)
            .FirstOrDefaultAsync(cancellationToken);

        return processus is null ? NotFound() : Ok(VersDto(processus));
    }

    [HttpPost]
    public async Task<ActionResult<ProcessusDto>> Creer(int projetId, UpsertProcessusDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "PROC", cancellationToken);

        var processus = new Processus { Code = code, ProjetId = projetId, Nom = dto.Nom, Declencheur = dto.Declencheur };

        db.Processus.Add(processus);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = processus.Id }, VersDto(processus));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int projetId, int id, UpsertProcessusDto dto, CancellationToken cancellationToken)
    {
        var processus = await db.Processus.FirstOrDefaultAsync(p => p.ProjetId == projetId && p.Id == id, cancellationToken);
        if (processus is null)
        {
            return NotFound();
        }

        processus.Nom = dto.Nom;
        processus.Declencheur = dto.Declencheur;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var processus = await db.Processus.FirstOrDefaultAsync(p => p.ProjetId == projetId && p.Id == id, cancellationToken);
        if (processus is null)
        {
            return NotFound();
        }

        db.Processus.Remove(processus);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // --- Étapes ---

    [HttpPost("{processusId:int}/etapes")]
    public async Task<ActionResult<EtapeProcessusDto>> AjouterEtape(
        int projetId, int processusId, UpsertEtapeProcessusDto dto, CancellationToken cancellationToken)
    {
        var processus = await db.Processus.FirstOrDefaultAsync(
            p => p.ProjetId == projetId && p.Id == processusId, cancellationToken);
        if (processus is null)
        {
            return NotFound();
        }

        var etape = new EtapeProcessus
        {
            ProcessusId = processusId,
            Ordre = dto.Ordre,
            Acteur = dto.Acteur,
            Action = dto.Action,
            Outil = dto.Outil,
            DureeEstimee = dto.DureeEstimee,
            ErreursConnues = dto.ErreursConnues,
            ExempleValide = dto.ExempleValide,
            ExempleDescription = dto.ExempleDescription
        };

        db.EtapesProcessus.Add(etape);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(VersDtoEtape(etape));
    }

    [HttpPut("{processusId:int}/etapes/{etapeId:int}")]
    public async Task<IActionResult> ModifierEtape(
        int projetId, int processusId, int etapeId, UpsertEtapeProcessusDto dto, CancellationToken cancellationToken)
    {
        var etape = await db.EtapesProcessus
            .Where(e => e.Id == etapeId && e.ProcessusId == processusId)
            .Where(e => e.Processus!.ProjetId == projetId)
            .FirstOrDefaultAsync(cancellationToken);

        if (etape is null)
        {
            return NotFound();
        }

        etape.Ordre = dto.Ordre;
        etape.Acteur = dto.Acteur;
        etape.Action = dto.Action;
        etape.Outil = dto.Outil;
        etape.DureeEstimee = dto.DureeEstimee;
        etape.ErreursConnues = dto.ErreursConnues;
        etape.ExempleValide = dto.ExempleValide;
        etape.ExempleDescription = dto.ExempleDescription;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{processusId:int}/etapes/{etapeId:int}")]
    public async Task<IActionResult> SupprimerEtape(
        int projetId, int processusId, int etapeId, CancellationToken cancellationToken)
    {
        var etape = await db.EtapesProcessus
            .Where(e => e.Id == etapeId && e.ProcessusId == processusId)
            .Where(e => e.Processus!.ProjetId == projetId)
            .FirstOrDefaultAsync(cancellationToken);

        if (etape is null)
        {
            return NotFound();
        }

        db.EtapesProcessus.Remove(etape);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static ProcessusDto VersDto(Processus p) => new(
        p.Id, p.Code, p.ProjetId, p.Nom, p.Declencheur,
        p.Etapes.OrderBy(e => e.Ordre).Select(VersDtoEtape).ToList());

    private static EtapeProcessusDto VersDtoEtape(EtapeProcessus e) => new(
        e.Id, e.ProcessusId, e.Ordre, e.Acteur, e.Action, e.Outil, e.DureeEstimee,
        e.ErreursConnues, e.ExempleValide, e.ExempleDescription);
}
