namespace Api.Data.Entities;

/// <summary>
/// Compteur dédié par ProjetId + préfixe (INF, Q, R, DEC, ...) pour générer des codes
/// auto-incrémentés qui ne sont jamais réutilisés, même après suppression d'une ligne
/// (Prompt Maître 4.3 : "auto-incrémentés par préfixe et par projet").
/// </summary>
public class CompteurCode
{
    public int Id { get; set; }
    public int ProjetId { get; set; }
    public required string Prefixe { get; set; }
    public int DernierNumero { get; set; }
}
