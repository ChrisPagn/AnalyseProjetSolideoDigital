using Shared.Enums;

namespace Shared.ModeEntretien;

/// <summary>
/// Une question du Guide des 18 phases (Bloc A), présentée une à la fois en mode entretien.
/// Chaque réponse crée directement une InformationRegistre (Prompt Maître : mécanique commune,
/// "Question posée → Réponse du client → Information détectée").
/// </summary>
public record QuestionGuidee(
    int Numero,
    string Libelle,
    string? Aide,
    SourceInformation SourceParDefaut,
    bool EstMultiligne);

/// <summary>
/// Questions principales des Phases 01 et 02 du Guide des 18 phases — seules phases dont les
/// questions ont une correspondance directe et unique vers InformationRegistre. Les Phases 03/04
/// (étapes de processus, problèmes quantifiés) restent gérées via les onglets Registres/Domaine
/// analysé existants (étapes 5/6) — décision validée avec l'utilisateur.
/// </summary>
public static class QuestionsGuideesParPhase
{
    public static readonly IReadOnlyList<QuestionGuidee> Phase01FicheClient =
    [
        new(1, "Nom de l'entreprise / organisation, secteur, taille", null, SourceInformation.Declaratif, false),
        new(2, "Nom et fonction de l'interlocuteur principal", null, SourceInformation.Declaratif, false),
        new(3, "Qui décide du lancement du projet ?", null, SourceInformation.Declaratif, false),
        new(4, "Qui valide les fonctionnalités ?", null, SourceInformation.Declaratif, false),
        new(5, "Qui utilisera l'outil au quotidien ?", null, SourceInformation.Declaratif, false),
        new(6, "Contexte d'origine : nouveau produit, remplacement d'un logiciel, automatisation, digitalisation ?",
            null, SourceInformation.Declaratif, true),
    ];

    public static readonly IReadOnlyList<QuestionGuidee> Phase02DecouverteProjet =
    [
        new(1, "Décrivez votre activité en 2-3 phrases, comme si je ne la connaissais pas.",
            null, SourceInformation.Declaratif, true),
        new(2, "Pourquoi ce projet aujourd'hui ? Quel événement a déclenché la réflexion ?",
            null, SourceInformation.Declaratif, true),
        new(3, "Résumez votre demande en une seule phrase.",
            "Une demande n'est pas un besoin validé — elle sera confrontée aux problèmes réels en Phase 04.",
            SourceInformation.Declaratif, true),
        new(4, "Si l'outil existait demain matin, qu'est-ce qui changerait concrètement dans votre journée ?",
            null, SourceInformation.Declaratif, true),
        new(5, "Que se passerait-il si rien n'était fait ?", null, SourceInformation.Declaratif, true),
    ];

    public static IReadOnlyList<QuestionGuidee>? PourPhase(int numeroPhase) => numeroPhase switch
    {
        1 => Phase01FicheClient,
        2 => Phase02DecouverteProjet,
        _ => null
    };
}
