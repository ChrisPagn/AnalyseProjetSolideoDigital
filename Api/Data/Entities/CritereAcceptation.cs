namespace Api.Data.Entities;

public class CritereAcceptation
{
    public int Id { get; set; }
    public int FonctionnaliteId { get; set; }
    public Fonctionnalite? Fonctionnalite { get; set; }

    public required string Given { get; set; }
    public required string When { get; set; }
    public required string Then { get; set; }
}
