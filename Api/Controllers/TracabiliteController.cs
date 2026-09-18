using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/tracabilite")]
public class TracabiliteController(
    AnalyseProjetDbContext db,
    TracabiliteService tracabiliteService,
    ContradictionDetectorService contradictionDetector,
    MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    /// <summary>
    /// Alertes structurelles (orphelins) — lecture seule, ne modifie rien (Prompt Maître 4.3).
    /// </summary>
    [HttpGet("alertes")]
    public async Task<ActionResult<IReadOnlyList<AlerteTracabiliteDto>>> GetAlertes(
        int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var alertes = await tracabiliteService.DetecterOrphelinsAsync(projetId, cancellationToken);
        return Ok(alertes);
    }

    /// <summary>
    /// Lance la détection des contradictions et génère les questions de relance manquantes
    /// (idempotent). Peut faire varier NiveauMaturite si une question Bloquante est créée.
    /// </summary>
    [HttpPost("detecter-contradictions")]
    public async Task<ActionResult<IReadOnlyList<ContradictionDto>>> DetecterContradictions(
        int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var contradictions = await contradictionDetector.DetecterEtGenererRelancesAsync(projetId, cancellationToken);

        if (contradictions.Count > 0)
        {
            await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Ok(contradictions);
    }
}
