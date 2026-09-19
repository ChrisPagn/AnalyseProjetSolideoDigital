using Api.Actions;
using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Projets;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets")]
public class ProjetsController(
    AnalyseProjetDbContext db,
    CreateProjetAction createProjetAction,
    MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjetDto>>> GetTous(CancellationToken cancellationToken)
    {
        var projets = await db.Projets
            .OrderByDescending(p => p.DateCreation)
            .Select(p => new ProjetDto(
                p.Id, p.Nom, p.ClientId, p.Client!.Nom, p.DateCreation, p.NiveauMaturite, p.StackEnvisagee, p.Statut,
                p.NotesPreparation))
            .ToListAsync(cancellationToken);

        return Ok(projets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjetDto>> GetParId(int id, CancellationToken cancellationToken)
    {
        var projet = await db.Projets
            .Where(p => p.Id == id)
            .Select(p => new ProjetDto(
                p.Id, p.Nom, p.ClientId, p.Client!.Nom, p.DateCreation, p.NiveauMaturite, p.StackEnvisagee, p.Statut,
                p.NotesPreparation))
            .FirstOrDefaultAsync(cancellationToken);

        return projet is null ? NotFound() : Ok(projet);
    }

    [HttpPost]
    public async Task<ActionResult<ProjetDto>> Creer(UpsertProjetDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Clients.AnyAsync(c => c.Id == dto.ClientId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.ClientId), "Le client indiqué n'existe pas.");
            return ValidationProblem(ModelState);
        }

        var projet = await createProjetAction.ExecuterAsync(dto, cancellationToken);
        var client = await db.Clients.FirstAsync(c => c.Id == projet.ClientId, cancellationToken);

        var resultDto = new ProjetDto(
            projet.Id, projet.Nom, projet.ClientId, client.Nom, projet.DateCreation,
            projet.NiveauMaturite, projet.StackEnvisagee, projet.Statut, projet.NotesPreparation);

        return CreatedAtAction(nameof(GetParId), new { id = projet.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int id, UpsertProjetDto dto, CancellationToken cancellationToken)
    {
        var projet = await db.Projets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (projet is null)
        {
            return NotFound();
        }

        if (!await db.Clients.AnyAsync(c => c.Id == dto.ClientId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.ClientId), "Le client indiqué n'existe pas.");
            return ValidationProblem(ModelState);
        }

        projet.Nom = dto.Nom;
        projet.ClientId = dto.ClientId;
        projet.StackEnvisagee = dto.StackEnvisagee;
        projet.Statut = dto.Statut;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int id, CancellationToken cancellationToken)
    {
        var projet = await db.Projets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (projet is null)
        {
            return NotFound();
        }

        db.Projets.Remove(projet);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Recalcule et renvoie le niveau de maturité actuel — utile pour rafraîchir l'UI sans
    /// attendre une autre opération qui le déclenche automatiquement.
    /// </summary>
    [HttpPost("{id:int}/recalculer-maturite")]
    public async Task<ActionResult<ProjetDto>> RecalculerMaturite(int id, CancellationToken cancellationToken)
    {
        var projetExiste = await db.Projets.AnyAsync(p => p.Id == id, cancellationToken);
        if (!projetExiste)
        {
            return NotFound();
        }

        await maturiteCalculator.RecalculerEtPersisterAsync(id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var projetDto = await db.Projets
            .Where(p => p.Id == id)
            .Select(p => new ProjetDto(
                p.Id, p.Nom, p.ClientId, p.Client!.Nom, p.DateCreation, p.NiveauMaturite, p.StackEnvisagee, p.Statut,
                p.NotesPreparation))
            .FirstAsync(cancellationToken);

        return Ok(projetDto);
    }

    /// <summary>
    /// Notes de préparation avant RDV — hors numérotation des 18 Phases, aucun impact sur la
    /// maturité (voir docs/03-proposition-phases-05-18-v2.md). Endpoint dédié plutôt que noyé
    /// dans Modifier/UpsertProjetDto : ce champ n'a pas de sens à la création du projet.
    /// </summary>
    [HttpPut("{id:int}/notes-preparation")]
    public async Task<IActionResult> ModifierNotesPreparation(
        int id, UpsertNotesPreparationDto dto, CancellationToken cancellationToken)
    {
        var projet = await db.Projets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (projet is null)
        {
            return NotFound();
        }

        projet.NotesPreparation = dto.NotesPreparation;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
