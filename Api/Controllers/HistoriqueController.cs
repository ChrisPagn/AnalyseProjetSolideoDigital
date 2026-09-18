using Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Registres;

namespace Api.Controllers;

/// <summary>
/// Consultation seule de l'historique — aucun endpoint de création/modification/suppression :
/// HistoriqueModification n'est alimenté que par AnalyseProjetDbContext.SaveChangesAsync
/// (journalisation automatique, section 5.5).
/// </summary>
[ApiController]
[Authorize]
[Route("api/historique")]
public class HistoriqueController(AnalyseProjetDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HistoriqueModificationDto>>> GetParEntite(
        [FromQuery] string entiteType, [FromQuery] int entiteId, CancellationToken cancellationToken)
    {
        var entrees = await db.HistoriqueModifications
            .Where(h => h.EntiteType == entiteType && h.EntiteId == entiteId)
            .OrderByDescending(h => h.DateModification)
            .Select(h => new HistoriqueModificationDto(
                h.Id, h.EntiteType, h.EntiteId, h.Champ, h.AncienneValeur, h.NouvelleValeur, h.ModifiePar, h.DateModification))
            .ToListAsync(cancellationToken);

        return Ok(entrees);
    }
}
