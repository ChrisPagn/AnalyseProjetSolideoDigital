using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/exports")]
public class ExportsController(
    AnalyseProjetDbContext db,
    MarkdownExportService markdownExportService,
    PromptMaitreTransfertService promptMaitreTransfertService) : ControllerBase
{
    [HttpGet("markdown")]
    public async Task<IActionResult> ExporterMarkdown(int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var (nomFichier, contenu) = await markdownExportService.GenererZipAsync(projetId, cancellationToken);

        return File(contenu, "application/zip", nomFichier);
    }

    /// <summary>
    /// Bloc de texte "prêt à coller" (Prompt Maître 4.4) — retourné en JSON, pas en fichier
    /// téléchargé : l'usage prévu est un copier-coller direct, pas un enregistrement sur disque.
    /// </summary>
    [HttpGet("prompt-maitre")]
    public async Task<ActionResult<PromptMaitreTransfertDto>> ExporterPromptMaitre(
        int projetId, CancellationToken cancellationToken)
    {
        if (!await db.Projets.AnyAsync(p => p.Id == projetId, cancellationToken))
        {
            return NotFound();
        }

        var contenu = await promptMaitreTransfertService.GenererAsync(projetId, cancellationToken);
        return Ok(new PromptMaitreTransfertDto(contenu));
    }
}
