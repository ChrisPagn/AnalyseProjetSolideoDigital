using System.Text;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Api.Services;

/// <summary>
/// Génère le bloc de texte "prêt à coller" décrit au Prompt Maître 4.4 : reprend les données du
/// projet mappées vers les sections 1, 2, 4, 9, 10 d'un prompt maître de développement
/// (Laravel/Blazor/SpringBoot selon Projet.StackEnvisagee).
/// Mapping validé avec l'utilisateur — voir chaque section pour la justification :
/// - §1 Contexte : Demande (Phase 02) + Problèmes les mieux priorisés (Phase 04/étape 6).
/// - §2 Stack : Projet.StackEnvisagee tel que saisi, en texte libre (l'outil n'impose pas de stack).
/// - §4 Domaine métier : Entite (étape 6) + Fonctionnalites groupées.
/// - §9 Rôles & permissions : Acteur → Permission (étape 6), correspondance directe.
/// - §10 Modules : proposition générée à partir des Fonctionnalites triées par PrioriteMoSCoW,
///   explicitement présentée comme une suggestion à ajuster (l'outil n'a pas connaissance des
///   dépendances techniques réelles entre modules).
/// </summary>
public class PromptMaitreTransfertService(AnalyseProjetDbContext db)
{
    private const string LibelleDemande = "Résumez votre demande en une seule phrase.";

    public async Task<string> GenererAsync(int projetId, CancellationToken cancellationToken = default)
    {
        var projet = await db.Projets
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == projetId, cancellationToken)
            ?? throw new InvalidOperationException($"Projet {projetId} introuvable.");

        var demande = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.Libelle == LibelleDemande)
            .Select(i => i.Valeur)
            .FirstOrDefaultAsync(cancellationToken);

        var problemesPriorises = await db.Problemes
            .Where(p => p.ProjetId == projetId)
            .ToListAsync(cancellationToken);
        problemesPriorises = problemesPriorises.OrderByDescending(p => p.ScoreCalcule).Take(3).ToList();

        var entites = await db.Entites.Where(e => e.ProjetId == projetId).ToListAsync(cancellationToken);

        var fonctionnalites = await db.Fonctionnalites
            .Where(f => f.ProjetId == projetId)
            .Include(f => f.Acteur)
            .ToListAsync(cancellationToken);

        var acteurs = await db.Acteurs
            .Where(a => a.ProjetId == projetId)
            .Include(a => a.Permissions)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();

        GenererSection1Contexte(sb, projet, demande, problemesPriorises);
        GenererSection2Stack(sb, projet);
        GenererSection4DomaineMetier(sb, entites, fonctionnalites);
        GenererSection9RolesPermissions(sb, acteurs);
        GenererSection10Modules(sb, fonctionnalites);

        return sb.ToString();
    }

    private static void GenererSection1Contexte(
        StringBuilder sb, Data.Entities.Projet projet, string? demande, List<Data.Entities.Probleme> problemesPriorises)
    {
        sb.AppendLine("## 1. CONTEXTE DU PROJET");
        sb.AppendLine();
        sb.AppendLine($"Tu es un expert {(string.IsNullOrWhiteSpace(projet.StackEnvisagee) ? "de la stack à définir" : projet.StackEnvisagee)}, " +
            "architecture logicielle et bonnes pratiques de développement. Tu vas m'aider à construire " +
            $"pas à pas une application professionnelle pour {projet.Client!.Nom}.");
        sb.AppendLine();
        sb.AppendLine("Objectif métier : " + (string.IsNullOrWhiteSpace(demande)
            ? "_À compléter — aucune demande n'a été validée en Phase 02._"
            : demande));
        sb.AppendLine();

        if (problemesPriorises.Count > 0)
        {
            sb.AppendLine("Problèmes principaux à résoudre (par ordre de priorité, Prompt Maître 4.3) :");
            foreach (var p in problemesPriorises)
            {
                sb.AppendLine($"- **{p.Code}** (score {p.ScoreCalcule}) : {p.Description}");
            }
        }
        else
        {
            sb.AppendLine("_Aucun problème quantifié n'a encore été enregistré (Phase 04)._");
        }
        sb.AppendLine();
    }

    private static void GenererSection2Stack(StringBuilder sb, Data.Entities.Projet projet)
    {
        sb.AppendLine("## 2. STACK TECHNIQUE");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(projet.StackEnvisagee)
            ? "_Stack non arbitrée — à compléter._"
            : $"Stack envisagée : **{projet.StackEnvisagee}**. Détailler les composants (frontend, " +
              "backend, base de données, déploiement) selon les contraintes du client.");
        sb.AppendLine();
    }

    private static void GenererSection4DomaineMetier(
        StringBuilder sb, List<Data.Entities.Entite> entites, List<Data.Entities.Fonctionnalite> fonctionnalites)
    {
        sb.AppendLine("## 4. DOMAINE MÉTIER");
        sb.AppendLine();
        sb.AppendLine("### 4.1 Entités principales");
        if (entites.Count == 0)
        {
            sb.AppendLine("_Aucune entité enregistrée (étape 6, onglet Entités)._");
        }
        else
        {
            sb.AppendLine("| Entité | Attributs clés |");
            sb.AppendLine("|---|---|");
            foreach (var e in entites)
            {
                sb.AppendLine($"| {e.Nom} | {e.Attributs ?? "_à préciser_"} |");
            }
        }
        sb.AppendLine();

        sb.AppendLine("### Fonctionnalités clés");
        if (fonctionnalites.Count == 0)
        {
            sb.AppendLine("_Aucune fonctionnalité enregistrée (étape 6, onglet Fonctionnalités)._");
        }
        else
        {
            foreach (var f in fonctionnalites.OrderBy(f => OrdrePriorite(f.Priorite)))
            {
                var acteur = f.Acteur is not null ? $" (acteur : {f.Acteur.Nom})" : "";
                sb.AppendLine($"- [{f.Priorite}] **{f.Code}** {f.Nom}{acteur}");
            }
        }
        sb.AppendLine();
    }

    private static void GenererSection9RolesPermissions(StringBuilder sb, List<Data.Entities.Acteur> acteurs)
    {
        sb.AppendLine("## 9. RÔLES & PERMISSIONS");
        sb.AppendLine();
        if (acteurs.Count == 0)
        {
            sb.AppendLine("_Aucun acteur enregistré (étape 6, onglet Acteurs)._");
        }
        else
        {
            sb.AppendLine("| Rôle | Accès |");
            sb.AppendLine("|---|---|");
            foreach (var a in acteurs)
            {
                var acces = a.Permissions.Count == 0
                    ? "_à préciser_"
                    : string.Join("; ", a.Permissions.Select(p => $"{p.EntiteConcernee}: {ResumerPermission(p)}"));
                sb.AppendLine($"| {a.Nom}{(string.IsNullOrWhiteSpace(a.Fonction) ? "" : $" ({a.Fonction})")} | {acces} |");
            }
        }
        sb.AppendLine();
    }

    private static void GenererSection10Modules(StringBuilder sb, List<Data.Entities.Fonctionnalite> fonctionnalites)
    {
        sb.AppendLine("## 10. MODULES — ORDRE DE DÉVELOPPEMENT (proposition, à ajuster)");
        sb.AppendLine();
        sb.AppendLine("_Suggestion générée à partir des priorités MoSCoW déclarées — l'outil n'a pas " +
            "connaissance des dépendances techniques réelles entre modules ; à revoir avec le " +
            "développeur avant de s'y fier._");
        sb.AppendLine();

        var fonctionnalitesTriees = fonctionnalites.OrderBy(f => OrdrePriorite(f.Priorite)).ToList();
        if (fonctionnalitesTriees.Count == 0)
        {
            sb.AppendLine("_Aucune fonctionnalité enregistrée pour proposer un ordre._");
        }
        else
        {
            var etape = 1;
            foreach (var f in fonctionnalitesTriees)
            {
                sb.AppendLine($"{etape}. **{f.Code}** {f.Nom} ({f.Priorite})");
                etape++;
            }
        }
    }

    private static int OrdrePriorite(PrioriteMoSCoW priorite) => priorite switch
    {
        PrioriteMoSCoW.MustHave => 0,
        PrioriteMoSCoW.ShouldHave => 1,
        PrioriteMoSCoW.CouldHave => 2,
        PrioriteMoSCoW.WontHave => 3,
        _ => 4
    };

    private static string ResumerPermission(Data.Entities.Permission p)
    {
        if (p.PeutVoir && p.PeutCreer && p.PeutModifier && p.PeutSupprimer && p.PeutValider)
        {
            return "Tout";
        }

        var actions = new List<string>();
        if (p.PeutVoir) actions.Add("Voir");
        if (p.PeutCreer) actions.Add("Créer");
        if (p.PeutModifier) actions.Add("Modifier");
        if (p.PeutSupprimer) actions.Add("Supprimer");
        if (p.PeutValider) actions.Add("Valider");

        return actions.Count == 0 ? "Aucun" : string.Join(",", actions);
    }
}
