using Api.Actions;
using Api.Data;
using Api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Clients;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clients")]
public class ClientsController(AnalyseProjetDbContext db, DeleteClientAction deleteClientAction) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetTous(CancellationToken cancellationToken)
    {
        var clients = await db.Clients
            .OrderBy(c => c.Nom)
            .Select(c => new ClientDto(
                c.Id, c.Nom, c.Secteur, c.Taille, c.Contact, c.Adresse, c.Notes, c.Projets.Count))
            .ToListAsync(cancellationToken);

        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> GetParId(int id, CancellationToken cancellationToken)
    {
        var client = await db.Clients
            .Where(c => c.Id == id)
            .Select(c => new ClientDto(
                c.Id, c.Nom, c.Secteur, c.Taille, c.Contact, c.Adresse, c.Notes, c.Projets.Count))
            .FirstOrDefaultAsync(cancellationToken);

        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Creer(UpsertClientDto dto, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Nom = dto.Nom,
            Secteur = dto.Secteur,
            Taille = dto.Taille,
            Contact = dto.Contact,
            Adresse = dto.Adresse,
            Notes = dto.Notes
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetParId), new { id = client.Id }, VersDto(client));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int id, UpsertClientDto dto, CancellationToken cancellationToken)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        client.Nom = dto.Nom;
        client.Secteur = dto.Secteur;
        client.Taille = dto.Taille;
        client.Contact = dto.Contact;
        client.Adresse = dto.Adresse;
        client.Notes = dto.Notes;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int id, CancellationToken cancellationToken)
    {
        var resultat = await deleteClientAction.ExecuterAsync(id, cancellationToken);

        return resultat switch
        {
            ResultatSuppression.Succes => NoContent(),
            ResultatSuppression.Introuvable => NotFound(),
            ResultatSuppression.Bloque => Conflict(
                "Ce client a des projets rattachés ; supprimez-les d'abord ou déplacez-les vers un autre client."),
            _ => Problem()
        };
    }

    private static ClientDto VersDto(Client c) => new(
        c.Id, c.Nom, c.Secteur, c.Taille, c.Contact, c.Adresse, c.Notes, c.Projets.Count);
}
