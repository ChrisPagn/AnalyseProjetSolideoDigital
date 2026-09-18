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
[Route("api/projets/{projetId:int}/questions")]
public class QuestionsRegistreController(
    AnalyseProjetDbContext db, CodeSequenceService codeSequence, MaturiteCalculatorService maturiteCalculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuestionRegistreDto>>> GetToutes(
        int projetId, [FromQuery] int? phaseId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var requete = db.QuestionsRegistre.Where(q => q.ProjetId == projetId);
        if (phaseId.HasValue)
        {
            requete = requete.Where(q => q.PhaseId == phaseId);
        }

        var questions = await requete
            .OrderBy(q => q.Id)
            .Select(q => new QuestionRegistreDto(
                q.Id, q.Code, q.ProjetId, q.PhaseId, q.Phase != null ? q.Phase.Numero : null,
                q.Question, q.Importance, q.Statut))
            .ToListAsync(cancellationToken);

        return Ok(questions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionRegistreDto>> GetParId(int projetId, int id, CancellationToken cancellationToken)
    {
        var question = await db.QuestionsRegistre
            .Where(q => q.ProjetId == projetId && q.Id == id)
            .Select(q => new QuestionRegistreDto(
                q.Id, q.Code, q.ProjetId, q.PhaseId, q.Phase != null ? q.Phase.Numero : null,
                q.Question, q.Importance, q.Statut))
            .FirstOrDefaultAsync(cancellationToken);

        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionRegistreDto>> Creer(
        int projetId, UpsertQuestionRegistreDto dto, CancellationToken cancellationToken)
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

        var code = await codeSequence.ProchainCodeAsync(projetId, "Q", cancellationToken);

        var question = new QuestionRegistre
        {
            Code = code,
            ProjetId = projetId,
            PhaseId = dto.PhaseId,
            Question = dto.Question,
            Importance = dto.Importance,
            Statut = dto.Statut
        };

        db.QuestionsRegistre.Add(question);

        // Une nouvelle question Bloquante+Ouverte peut plafonner NiveauMaturite à 1
        // (Prompt Maître 4.3) : recalcul dans la même opération (section 5.4).
        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var numeroPhase = dto.PhaseId.HasValue
            ? await db.Phases.Where(p => p.Id == dto.PhaseId).Select(p => (int?)p.Numero).FirstOrDefaultAsync(cancellationToken)
            : null;

        var resultDto = new QuestionRegistreDto(
            question.Id, question.Code, question.ProjetId, question.PhaseId, numeroPhase,
            question.Question, question.Importance, question.Statut);

        return CreatedAtAction(nameof(GetParId), new { projetId, id = question.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(
        int projetId, int id, UpsertQuestionRegistreDto dto, CancellationToken cancellationToken)
    {
        var question = await db.QuestionsRegistre.FirstOrDefaultAsync(
            q => q.ProjetId == projetId && q.Id == id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        if (dto.PhaseId.HasValue && !await db.Phases.AnyAsync(
                p => p.Id == dto.PhaseId && p.ProjetId == projetId, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.PhaseId), "La phase indiquée n'existe pas pour ce projet.");
            return ValidationProblem(ModelState);
        }

        question.PhaseId = dto.PhaseId;
        question.Question = dto.Question;
        question.Importance = dto.Importance;
        question.Statut = dto.Statut;

        await db.SaveChangesAsync(cancellationToken);

        // Une question qui passe Ouverte<->Resolue (ou change d'Importance) peut faire varier
        // NiveauMaxAutorise (Prompt Maître 4.3) : recalcul après persistance du changement.
        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int projetId, int id, CancellationToken cancellationToken)
    {
        var question = await db.QuestionsRegistre.FirstOrDefaultAsync(
            q => q.ProjetId == projetId && q.Id == id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        db.QuestionsRegistre.Remove(question);
        await db.SaveChangesAsync(cancellationToken);

        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
