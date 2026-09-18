using System.Globalization;
using System.IO.Compression;
using System.Text;
using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Api.Services;

/// <summary>
/// Génère le dossier clients/[nom-client]/ décrit au Prompt Maître 4.4 : ANALYSE.md (synthèse)
/// + un fichier par phase (01-fiche-client.md → 18-transfertprompt-maitre.md), livré sous forme
/// d'archive ZIP téléchargée par le navigateur — aucune écriture sur le système de fichiers du
/// serveur (décision validée avec l'utilisateur : cohérent avec un déploiement conteneurisé et
/// un usage nomade en RDV client).
/// </summary>
public class MarkdownExportService(AnalyseProjetDbContext db, TracabiliteService tracabiliteService)
{
    public async Task<(string NomFichier, byte[] Contenu)> GenererZipAsync(
        int projetId, CancellationToken cancellationToken = default)
    {
        var projet = await db.Projets
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == projetId, cancellationToken)
            ?? throw new InvalidOperationException($"Projet {projetId} introuvable.");

        var phases = await db.Phases
            .Where(p => p.ProjetId == projetId)
            .OrderBy(p => p.Numero)
            .ToListAsync(cancellationToken);

        var informations = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId)
            .ToListAsync(cancellationToken);

        var questions = await db.QuestionsRegistre
            .Where(q => q.ProjetId == projetId)
            .ToListAsync(cancellationToken);

        // RisqueRegistre et DecisionRegistre n'ont pas de PhaseId dans le modèle (Prompt Maître
        // 4.1) : ils sont listés uniquement dans ANALYSE.md, jamais répétés dans un fichier de
        // phase (décision validée avec l'utilisateur).
        var risques = await db.RisquesRegistre.Where(r => r.ProjetId == projetId).ToListAsync(cancellationToken);
        var decisions = await db.DecisionsRegistre.Where(d => d.ProjetId == projetId).ToListAsync(cancellationToken);
        var problemes = await db.Problemes.Where(p => p.ProjetId == projetId).ToListAsync(cancellationToken);

        var alertes = await tracabiliteService.DetecterOrphelinsAsync(projetId, cancellationToken);

        using var memoire = new MemoryStream();
        using (var archive = new ZipArchive(memoire, ZipArchiveMode.Create, leaveOpen: true))
        {
            AjouterFichier(archive, "ANALYSE.md", GenererAnalyseMd(projet, phases, problemes, risques, decisions, alertes));

            foreach (var phase in phases)
            {
                var informationsPhase = informations.Where(i => i.PhaseId == phase.Id).ToList();
                var questionsOuvertesPhase = questions.Where(q => q.PhaseId == phase.Id && q.Statut == StatutQuestion.Ouverte).ToList();

                var nomFichier = $"{phase.Numero:D2}-{Sluggifier(phase.Nom)}.md";
                AjouterFichier(archive, nomFichier, GenererFichierPhase(phase, informationsPhase, questionsOuvertesPhase));
            }
        }

        var nomClientSlug = Sluggifier(projet.Client!.Nom);
        return ($"analyse-{nomClientSlug}.zip", memoire.ToArray());
    }

    private static readonly UTF8Encoding Utf8SansBom = new(encoderShouldEmitUTF8Identifier: false);

    private static void AjouterFichier(ZipArchive archive, string nom, string contenu)
    {
        var entree = archive.CreateEntry(nom, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entree.Open(), Utf8SansBom);
        writer.Write(contenu);
    }

    private static string GenererAnalyseMd(
        Projet projet, List<Phase> phases, List<Probleme> problemes,
        List<RisqueRegistre> risques, List<DecisionRegistre> decisions, IReadOnlyList<Shared.Dtos.Domaine.AlerteTracabiliteDto> alertes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {projet.Client!.Nom} — {projet.Nom}");
        sb.AppendLine();

        sb.AppendLine("## Fiche client");
        sb.AppendLine($"- **Client** : {projet.Client.Nom}");
        if (!string.IsNullOrWhiteSpace(projet.Client.Secteur))
        {
            sb.AppendLine($"- **Secteur** : {projet.Client.Secteur}");
        }
        if (!string.IsNullOrWhiteSpace(projet.Client.Taille))
        {
            sb.AppendLine($"- **Taille** : {projet.Client.Taille}");
        }
        if (!string.IsNullOrWhiteSpace(projet.Client.Contact))
        {
            sb.AppendLine($"- **Contact** : {projet.Client.Contact}");
        }
        sb.AppendLine($"- **Stack envisagée** : {projet.StackEnvisagee ?? "Non arbitrée"}");
        sb.AppendLine();

        sb.AppendLine($"## Niveau de maturité : {LibelleMaturite(projet.NiveauMaturite)}");
        sb.AppendLine();

        var demande = phases.FirstOrDefault(p => p.Numero == 2);
        sb.AppendLine("## Demande exprimée vs Problème pressenti");
        sb.AppendLine(demande?.Statut == StatutPhase.Terminee
            ? "Phase 02 terminée — voir le fichier `02-decouverte-du-projet.md` pour le détail."
            : "Phase 02 (Découverte du projet) non terminée — demande pas encore confrontée aux problèmes réels.");
        sb.AppendLine();

        sb.AppendLine("## Problèmes priorisés");
        if (problemes.Count == 0)
        {
            sb.AppendLine("_Aucun problème enregistré._");
        }
        else
        {
            sb.AppendLine("| Code | Description | Gravité | Score | Couvert |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var p in problemes.OrderByDescending(p => p.ScoreCalcule))
            {
                var couvert = p.LiensTracabilite.Count > 0 ? "✅" : "⛔";
                sb.AppendLine($"| {p.Code} | {p.Description} | {p.Gravite} | {p.ScoreCalcule} | {couvert} |");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Risques");
        if (risques.Count == 0)
        {
            sb.AppendLine("_Aucun risque enregistré._");
        }
        else
        {
            foreach (var r in risques)
            {
                sb.AppendLine($"- **{r.Code}** — {r.Description}" +
                    (string.IsNullOrWhiteSpace(r.Mesure) ? "" : $" (mesure : {r.Mesure})"));
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Décisions");
        if (decisions.Count == 0)
        {
            sb.AppendLine("_Aucune décision enregistrée._");
        }
        else
        {
            foreach (var d in decisions)
            {
                sb.AppendLine($"- **{d.Code}** — {d.Description}" +
                    (string.IsNullOrWhiteSpace(d.Justification) ? "" : $" (justification : {d.Justification})"));
            }
        }
        sb.AppendLine();

        sb.AppendLine("## État des 18 phases");
        foreach (var phase in phases)
        {
            sb.AppendLine($"- Phase {phase.Numero:D2} — {phase.Nom} : {LibelleStatutPhase(phase.Statut)}");
        }
        sb.AppendLine();

        sb.AppendLine("## Alertes de traçabilité");
        if (alertes.Count == 0)
        {
            sb.AppendLine("_Aucune alerte._");
        }
        else
        {
            foreach (var alerte in alertes)
            {
                sb.AppendLine($"- {alerte.Message}");
            }
        }

        return sb.ToString();
    }

    private static string GenererFichierPhase(
        Phase phase, List<InformationRegistre> informations, List<QuestionRegistre> questionsOuvertes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Phase {phase.Numero:D2} — {phase.Nom}");
        sb.AppendLine();

        sb.AppendLine("## Objectif de la phase");
        sb.AppendLine("_Voir le Guide des 18 phases pour l'objectif détaillé de cette phase._");
        sb.AppendLine();

        sb.AppendLine("## Informations validées (✅)");
        var validees = informations.Where(i => i.Statut == StatutInformation.Valide).ToList();
        if (validees.Count == 0)
        {
            sb.AppendLine("_Aucune information validée._");
        }
        else
        {
            foreach (var info in validees)
            {
                sb.AppendLine($"- **{info.Code}** {info.Libelle} : {info.Valeur}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Points ouverts (⚠️/⛔)");
        var aConfirmer = informations.Where(i => i.Statut == StatutInformation.AConfirmer).ToList();
        if (aConfirmer.Count == 0 && questionsOuvertes.Count == 0)
        {
            sb.AppendLine("_Aucun point ouvert._");
        }
        else
        {
            foreach (var info in aConfirmer)
            {
                sb.AppendLine($"- ⚠️ **{info.Code}** {info.Libelle} : {info.Valeur} (à confirmer)");
            }
            foreach (var question in questionsOuvertes)
            {
                var symbole = question.Importance == ImportanceQuestion.Bloquante ? "⛔" : "⚠️";
                sb.AppendLine($"- {symbole} **{question.Code}** {question.Question}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Décisions prises");
        sb.AppendLine("_Les décisions ne sont pas rattachées à une phase précise — voir ANALYSE.md._");

        return sb.ToString();
    }

    private static string LibelleMaturite(NiveauMaturite niveau) => niveau switch
    {
        NiveauMaturite.Niveau0Inconnu => "Niveau 0 — Inconnu",
        NiveauMaturite.Niveau1Comprehension => "Niveau 1 — Compréhension",
        NiveauMaturite.Niveau2AnalyseMetier => "Niveau 2 — Analyse métier",
        NiveauMaturite.Niveau3Specification => "Niveau 3 — Spécification",
        NiveauMaturite.Niveau4Conception => "Niveau 4 — Conception",
        NiveauMaturite.Niveau5PretPourDev => "Niveau 5 — Prêt pour dev",
        _ => niveau.ToString()
    };

    private static string LibelleStatutPhase(StatutPhase statut) => statut switch
    {
        StatutPhase.NonCommencee => "Non commencée",
        StatutPhase.EnCours => "En cours",
        StatutPhase.Terminee => "Terminée ✅",
        _ => statut.ToString()
    };

    /// <summary>Convertit un libellé (ex. "Découverte du projet") en slug de nom de fichier ASCII.</summary>
    private static string Sluggifier(string texte)
    {
        var normalise = texte.Normalize(NormalizationForm.FormD);
        var sansAccents = new StringBuilder();
        foreach (var c in normalise)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sansAccents.Append(c);
            }
        }

        var minuscule = sansAccents.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var slug = new StringBuilder();
        var dernierTiret = false;

        foreach (var c in minuscule)
        {
            if (char.IsLetterOrDigit(c))
            {
                slug.Append(c);
                dernierTiret = false;
            }
            else if (!dernierTiret && slug.Length > 0)
            {
                slug.Append('-');
                dernierTiret = true;
            }
        }

        return slug.ToString().TrimEnd('-');
    }
}
