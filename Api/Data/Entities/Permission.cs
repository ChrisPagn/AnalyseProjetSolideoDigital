namespace Api.Data.Entities;

/// <summary>
/// Permissions du domaine client analysé — à ne pas confondre avec les rôles AnalyseProjet
/// (Prompt Maître section 9).
/// </summary>
public class Permission
{
    public int Id { get; set; }
    public int ActeurId { get; set; }
    public Acteur? Acteur { get; set; }

    public required string EntiteConcernee { get; set; }
    public bool PeutVoir { get; set; }
    public bool PeutCreer { get; set; }
    public bool PeutModifier { get; set; }
    public bool PeutSupprimer { get; set; }
    public bool PeutValider { get; set; }
}
