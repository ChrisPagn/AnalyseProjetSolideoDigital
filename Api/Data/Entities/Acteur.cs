namespace Api.Data.Entities;

/// <summary>
/// Acteur du domaine métier ANALYSÉ chez le client — à ne pas confondre avec les rôles
/// AnalyseProjet (Identity), voir Prompt Maître section 9.
/// </summary>
public class Acteur
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. ACT-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Nom { get; set; }
    public string? Fonction { get; set; }

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<Fonctionnalite> Fonctionnalites { get; set; } = new List<Fonctionnalite>();
}
