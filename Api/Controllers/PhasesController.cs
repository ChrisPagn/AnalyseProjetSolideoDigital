using Api.Actions;
using Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Phases;
using Shared.Dtos.Projets;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/phases")]
public class PhasesController(AnalyseProjetDbContext db, UpdateStatutPhaseAction updateStatutPhaseAction) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PhaseDto>>> GetToutes(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var phases = await db.Phases
            .Where(p => p.ProjetId == projetId)
            .OrderBy(p => p.Numero)
            .Select(p => new PhaseDto(p.Id, p.ProjetId, p.Numero, p.Nom, p.Statut, p.DateMaj))
            .ToListAsync(cancellationToken);

        return Ok(phases);
    }

    [HttpGet("{numero:int}")]
    public async Task<ActionResult<PhaseDto>> GetParNumero(int projetId, int numero, CancellationToken cancellationToken)
    {
        var phase = await db.Phases
            .Where(p => p.ProjetId == projetId && p.Numero == numero)
            .Select(p => new PhaseDto(p.Id, p.ProjetId, p.Numero, p.Nom, p.Statut, p.DateMaj))
            .FirstOrDefaultAsync(cancellationToken);

        return phase is null ? NotFound() : Ok(phase);
    }

    [HttpPut("{phaseId:int}/statut")]
    public async Task<ActionResult<ProjetDto>> ModifierStatut(
        int projetId, int phaseId, UpdateStatutPhaseDto dto, CancellationToken cancellationToken)
    {
        var resultat = await updateStatutPhaseAction.ExecuterAsync(projetId, phaseId, dto.Statut, cancellationToken);

        if (resultat == ResultatMajStatutPhase.Introuvable)
        {
            return NotFound();
        }

        var projetDto = await db.Projets
            .Where(p => p.Id == projetId)
            .Select(p => new ProjetDto(
                p.Id, p.Nom, p.ClientId, p.Client!.Nom, p.DateCreation, p.NiveauMaturite, p.StackEnvisagee, p.Statut))
            .FirstAsync(cancellationToken);

        return Ok(projetDto);
    }
}
