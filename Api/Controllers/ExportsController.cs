using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projets/{projetId:int}/exports")]
public class ExportsController(AnalyseProjetDbContext db, MarkdownExportService markdownExportService) : ControllerBase
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
}
