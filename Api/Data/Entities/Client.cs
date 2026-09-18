namespace Api.Data.Entities;

public class Client
{
    public int Id { get; set; }
    public required string Nom { get; set; }
    public string? Secteur { get; set; }
    public string? Taille { get; set; }
    public string? Contact { get; set; }
    public string? Adresse { get; set; }
    public string? Notes { get; set; }

    public ICollection<Projet> Projets { get; set; } = new List<Projet>();
}
