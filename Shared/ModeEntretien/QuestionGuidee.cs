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

    /// <summary>
    /// Phase 10 — Contraintes (docs/03-proposition-phases-05-18-v2.md) : ce qui s'impose au
    /// projet de l'extérieur (budget, délai, migration, contraintes techniques/réglementaires).
    /// </summary>
    public static readonly IReadOnlyList<QuestionGuidee> Phase10Contraintes =
    [
        new(1, "Y a-t-il un budget déjà évoqué ou fixé pour ce projet ?",
            "Reprendre/trancher la réponse \"à confirmer\" de la Phase 01 si elle existe.",
            SourceInformation.Declaratif, false),
        new(2, "Y a-t-il un délai ou une échéance à respecter ?", null, SourceInformation.Declaratif, false),
        new(3, "Les documents/outils identifiés en Phase 07 doivent-ils être repris ou migrés ? Sous quelle forme ?",
            "Chaque document Phase 07 marqué \"à conserver\"/\"existant\" est un candidat direct.",
            SourceInformation.Declaratif, true),
        new(4, "Existe-t-il une contrainte technique déjà connue (hébergement imposé, système existant à interfacer) ?",
            null, SourceInformation.Declaratif, true),
        new(5, "Existe-t-il une contrainte réglementaire connue (RGPD, obligation sectorielle) ?",
            null, SourceInformation.Declaratif, true),
    ];

    /// <summary>
    /// Phase 11 — Règles métier (docs/03-proposition-phases-05-18-v2.md) : règles de gestion
    /// (calculs, seuils, cas particuliers) à ne pas redécouvrir pendant le développement.
    /// </summary>
    public static readonly IReadOnlyList<QuestionGuidee> Phase11ReglesMetier =
    [
        new(1, "Existe-t-il des règles de calcul ou des seuils à respecter ?",
            "Ex. une remise automatique au-delà d'un montant, une relance après un délai fixe.",
            SourceInformation.Declaratif, true),
        new(2, "Existe-t-il des cas particuliers ou exceptions aux règles générales décrites en Phase 03 ?",
            null, SourceInformation.Declaratif, true),
        new(3, "Que se passe-t-il dans les cas limites (montant à zéro, deux personnes agissent en même temps) ?",
            "Toute réponse \"ça dépend de la personne / pas de règle fixe\" en Phase 03 doit être reprise et clarifiée ici, ou explicitement actée comme non formalisée.",
            SourceInformation.Declaratif, true),
    ];

    /// <summary>
    /// Phase 12 — Intégrations (docs/03-proposition-phases-05-18-v2.md) : systèmes externes avec
    /// lesquels l'outil futur devra communiquer.
    /// </summary>
    public static readonly IReadOnlyList<QuestionGuidee> Phase12Integrations =
    [
        new(1, "L'outil doit-il échanger des données avec un système déjà existant (comptabilité, CRM, banque...) ?",
            "Reprendre les systèmes/outils déjà cités en Phase 07 (documents) et Phase 10 (contraintes techniques).",
            SourceInformation.Declaratif, true),
        new(2, "Dans quel sens (l'outil envoie / reçoit / les deux) ?", null, SourceInformation.Declaratif, false),
        new(3, "Cet échange doit-il être automatique, ou un export/import manuel suffit-il pour l'instant ?",
            null, SourceInformation.Declaratif, false),
        new(4, "Existe-t-il déjà un accès technique documenté à ce système (API, export possible) ?",
            null, SourceInformation.Declaratif, true),
    ];

    /// <summary>
    /// Phase 13 — Exigences non-fonctionnelles (docs/03-proposition-phases-05-18-v2.md) :
    /// qualités attendues de l'outil (performance, disponibilité, sécurité, volume).
    /// </summary>
    public static readonly IReadOnlyList<QuestionGuidee> Phase13ExigencesNonFonctionnelles =
    [
        new(1, "Combien de personnes utiliseront l'outil, et à quelle fréquence ?", null, SourceInformation.Declaratif, false),
        new(2, "Y a-t-il un besoin de disponibilité particulier (horaires de bureau, ou accès 24/7) ?",
            null, SourceInformation.Declaratif, false),
        new(3, "Y a-t-il une contrainte de sécurité particulière (données sensibles, RGPD déjà évoqué en Phase 10) ?",
            null, SourceInformation.Declaratif, true),
        new(4, "Y a-t-il un besoin d'accès mobile / hors ligne ?", null, SourceInformation.Declaratif, false),
        new(5, "Quel est le volume de données prévisible (nombre de dossiers/an, croissance attendue) ?",
            null, SourceInformation.Declaratif, true),
    ];

    /// <summary>
    /// Phase 15 — Planning (docs/03-proposition-phases-05-18-v2.md) : séquencement réaliste des
    /// lots de développement, sans devenir un outil de gestion de projet à part entière.
    /// </summary>
    public static readonly IReadOnlyList<QuestionGuidee> Phase15Planning =
    [
        new(1, "Existe-t-il une échéance externe contraignante ?",
            "Reprend/confirme la réponse de la Phase 10.", SourceInformation.Declaratif, false),
        new(2, "Les fonctionnalités Must Have (Phase 14) peuvent-elles être livrées en un seul lot, ou faut-il découper ?",
            null, SourceInformation.Declaratif, true),
        new(3, "Y a-t-il une dépendance connue entre certaines fonctionnalités (l'une doit exister avant que l'autre ait du sens) ?",
            null, SourceInformation.Declaratif, true),
    ];

    public static IReadOnlyList<QuestionGuidee>? PourPhase(int numeroPhase) => numeroPhase switch
    {
        1 => Phase01FicheClient,
        2 => Phase02DecouverteProjet,
        10 => Phase10Contraintes,
        11 => Phase11ReglesMetier,
        12 => Phase12Integrations,
        13 => Phase13ExigencesNonFonctionnelles,
        15 => Phase15Planning,
        _ => null
    };
}
