namespace Api.Data.Entities;

public class Processus
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. PROC-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Nom { get; set; }
    public string? Declencheur { get; set; }

    public ICollection<EtapeProcessus> Etapes { get; set; } = new List<EtapeProcessus>();
}
