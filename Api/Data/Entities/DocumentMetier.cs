namespace Api.Data.Entities;

public class DocumentMetier
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. DOC-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Type { get; set; }
    public string? Origine { get; set; }
    public string? Destination { get; set; }
    public string? Format { get; set; }
    public string? DureeConservation { get; set; }
}
