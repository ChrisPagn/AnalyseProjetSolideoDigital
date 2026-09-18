using Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Dashboard;
using Shared.Enums;

namespace Api.Controllers;

/// <summary>
/// Vue d'ensemble multi-projets (Prompt Maître, section 10 — étape N) : lecture seule, agrège des
/// données déjà exposées par ailleurs (Projets, QuestionRegistre) sans logique métier nouvelle.
/// </summary>
[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(AnalyseProjetDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken cancellationToken)
    {
        var projets = await db.Projets
            .Include(p => p.Client)
            .Select(p => new
            {
                p.Id,
                p.Nom,
                ClientNom = p.Client!.Nom,
                p.NiveauMaturite,
                p.Statut,
                NombrePhasesTerminees = p.Phases.Count(ph => ph.Statut == StatutPhase.Terminee),
                NombreQuestionsBloquantesOuvertes = p.Questions.Count(
                    q => q.Importance == ImportanceQuestion.Bloquante && q.Statut == StatutQuestion.Ouverte)
            })
            .OrderByDescending(p => p.NombreQuestionsBloquantesOuvertes)
            .ThenBy(p => p.Nom)
            .ToListAsync(cancellationToken);

        var projetsResume = projets
            .Select(p => new ProjetResumeDto(
                p.Id, p.Nom, p.ClientNom, p.NiveauMaturite, p.Statut,
                p.NombrePhasesTerminees, p.NombreQuestionsBloquantesOuvertes))
            .ToList();

        var questionsBloquantes = await db.QuestionsRegistre
            .Where(q => q.Importance == ImportanceQuestion.Bloquante && q.Statut == StatutQuestion.Ouverte)
            .Select(q => new QuestionBloquanteDto(q.ProjetId, q.Projet!.Nom, q.Code, q.Question))
            .ToListAsync(cancellationToken);

        return Ok(new DashboardDto(projetsResume, questionsBloquantes));
    }
}
