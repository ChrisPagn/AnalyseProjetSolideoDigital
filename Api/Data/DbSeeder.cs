using Api.Data.Entities;
using Shared.Enums;

namespace Api.Data;

/// <summary>
/// Données de test réalistes pour le développement — 3 projets fictifs à des stades différents
/// du protocole d'analyse. Appelé uniquement en environnement Development (voir Program.cs).
/// </summary>
public static class DbSeeder
{
    public static void Seed(AnalyseProjetDbContext db)
    {
        if (db.Clients.Any())
        {
            return;
        }

        // --- Client 1 : cabinet comptable, projet au tout début (Niveau 0) ---
        var clientCompta = new Client
        {
            Nom = "Cabinet Verdier & Associés",
            Secteur = "Comptabilité",
            Taille = "8 salariés",
            Contact = "Sophie Verdier, gérante",
            Adresse = "12 rue des Tilleuls, 69003 Lyon",
            Notes = "Premier contact via recommandation client existant."
        };

        var projetCompta = new Projet
        {
            Nom = "Digitalisation suivi dossiers clients",
            Client = clientCompta,
            DateCreation = DateTime.UtcNow.AddDays(-3),
            NiveauMaturite = NiveauMaturite.Niveau0Inconnu,
            StackEnvisagee = "Non arbitré",
            Statut = StatutProjet.EnCours
        };

        projetCompta.Phases.Add(new Phase { Numero = 1, Nom = "Fiche client", Statut = StatutPhase.EnCours, DateMaj = DateTime.UtcNow });
        for (var n = 2; n <= 18; n++)
        {
            projetCompta.Phases.Add(new Phase { Numero = n, Nom = NomPhase(n), Statut = StatutPhase.NonCommencee, DateMaj = DateTime.UtcNow });
        }

        // --- Client 2 : artisan BTP, Bloc A en cours (Niveau 1) ---
        var clientBtp = new Client
        {
            Nom = "Martin Rénovation",
            Secteur = "Bâtiment / Rénovation",
            Taille = "3 salariés",
            Contact = "Karim Martin, gérant",
            Adresse = "5 impasse des Forgerons, 42000 Saint-Étienne",
            Notes = "Utilise encore Excel + carnet papier pour les devis."
        };

        var projetBtp = new Projet
        {
            Nom = "Outil de suivi devis et chantiers",
            Client = clientBtp,
            DateCreation = DateTime.UtcNow.AddDays(-15),
            NiveauMaturite = NiveauMaturite.Niveau1Comprehension,
            StackEnvisagee = "Laravel",
            Statut = StatutProjet.EnCours
        };

        projetBtp.Phases.Add(new Phase { Numero = 1, Nom = "Fiche client", Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow.AddDays(-14) });
        projetBtp.Phases.Add(new Phase { Numero = 2, Nom = "Découverte du projet", Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow.AddDays(-10) });
        projetBtp.Phases.Add(new Phase { Numero = 3, Nom = "Processus métier", Statut = StatutPhase.EnCours, DateMaj = DateTime.UtcNow.AddDays(-2) });
        for (var n = 4; n <= 18; n++)
        {
            projetBtp.Phases.Add(new Phase { Numero = n, Nom = NomPhase(n), Statut = StatutPhase.NonCommencee, DateMaj = DateTime.UtcNow });
        }

        projetBtp.Informations.Add(new InformationRegistre
        {
            Code = "INF-001",
            Libelle = "Nom entreprise",
            Valeur = "Martin Rénovation",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });
        projetBtp.Informations.Add(new InformationRegistre
        {
            Code = "INF-002",
            Libelle = "Décideur final",
            Valeur = "Karim Martin",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });

        var procDevis = new Processus
        {
            Code = "PROC-001",
            Nom = "Établissement d'un devis",
            Declencheur = "Appel ou visite d'un client potentiel"
        };
        procDevis.Etapes.Add(new EtapeProcessus
        {
            Ordre = 1,
            Acteur = "Karim Martin",
            Action = "Visite du chantier et prise de mesures",
            Outil = "Carnet papier",
            DureeEstimee = "45 min",
            ErreursConnues = "Mesures parfois illisibles au moment de la saisie",
            ExempleValide = true,
            ExempleDescription = "Devis Dupont du 12/09 vérifié sur place avec le carnet original."
        });
        procDevis.Etapes.Add(new EtapeProcessus
        {
            Ordre = 2,
            Acteur = "Karim Martin",
            Action = "Saisie du devis dans le fichier Excel modèle",
            Outil = "Excel",
            DureeEstimee = "30 min",
            ErreursConnues = "Formules cassées si une ligne est mal insérée",
            ExempleValide = false
        });
        projetBtp.Processus.Add(procDevis);

        projetBtp.Questions.Add(new QuestionRegistre
        {
            Code = "Q-001",
            Question = "Qui remplace Karim Martin en cas d'absence pour valider un devis urgent ?",
            Importance = ImportanceQuestion.Bloquante,
            Statut = StatutQuestion.Ouverte
        });

        // --- Client 3 : association loi 1901, Bloc A terminé (Niveau 2) ---
        var clientAsso = new Client
        {
            Nom = "Association Vivre Ensemble",
            Secteur = "Associatif / Social",
            Taille = "2 salariés + 15 bénévoles",
            Contact = "Fatima Nasser, présidente",
            Adresse = "22 avenue de la République, 75011 Paris",
            Notes = "Budget serré, forte sensibilité au coût de maintenance."
        };

        var projetAsso = new Projet
        {
            Nom = "Gestion adhésions et dons",
            Client = clientAsso,
            DateCreation = DateTime.UtcNow.AddDays(-40),
            NiveauMaturite = NiveauMaturite.Niveau2AnalyseMetier,
            StackEnvisagee = "Blazor",
            Statut = StatutProjet.EnCours
        };

        for (var n = 1; n <= 4; n++)
        {
            projetAsso.Phases.Add(new Phase { Numero = n, Nom = NomPhase(n), Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow.AddDays(-40 + n * 3) });
        }
        for (var n = 5; n <= 18; n++)
        {
            projetAsso.Phases.Add(new Phase { Numero = n, Nom = NomPhase(n), Statut = StatutPhase.NonCommencee, DateMaj = DateTime.UtcNow });
        }

        var probRelance = new Probleme
        {
            Code = "PROB-001",
            Description = "Relances de cotisations impayées faites manuellement, souvent oubliées",
            Gravite = Gravite.Important,
            Frequence = 12,
            ImpactTempsHeuresMois = 6,
            CoutEstime = null,
            ScoreCalcule = 72
        };
        projetAsso.Problemes.Add(probRelance);

        var acteurTresoriere = new Acteur
        {
            Code = "ACT-001",
            Nom = "Trésorière bénévole",
            Fonction = "Suivi des cotisations et dons"
        };
        projetAsso.Acteurs.Add(acteurTresoriere);

        var fonctionnaliteRelance = new Fonctionnalite
        {
            Code = "F-001",
            Nom = "Relance automatique des cotisations impayées",
            Description = "Envoi d'un e-mail de relance 30 jours après échéance non payée",
            Priorite = PrioriteMoSCoW.MustHave,
            Statut = StatutFonctionnalite.Identifiee,
            Acteur = acteurTresoriere
        };
        projetAsso.Fonctionnalites.Add(fonctionnaliteRelance);

        projetAsso.Problemes.First().LiensTracabilite.Add(new LienTracabilite
        {
            Probleme = probRelance,
            Fonctionnalite = fonctionnaliteRelance
        });

        db.Clients.AddRange(clientCompta, clientBtp, clientAsso);
        db.Projets.AddRange(projetCompta, projetBtp, projetAsso);
        db.SaveChanges();
    }

    /// <summary>
    /// Seules les phases 1-4 (Bloc A) sont nommées et détaillées dans le Guide des 18 phases fourni.
    /// Les noms 5-18 ci-dessous sont des libellés provisoires déduits des renvois "🔗 Liens" du
    /// Bloc A (ex. "05 Acteurs", "10 Contraintes") — à valider/corriger quand les Blocs B à E
    /// seront rédigés dans le Guide, avant de s'y fier pour l'UI finale.
    /// </summary>
    private static string NomPhase(int numero) => numero switch
    {
        1 => "Fiche client",
        2 => "Découverte du projet",
        3 => "Processus métier",
        4 => "Problèmes et besoins",
        5 => "Acteurs",
        6 => "Données",
        7 => "Documents (provisoire)",
        8 => "Fonctionnalités (provisoire)",
        9 => "Automatisations (provisoire)",
        10 => "Contraintes (provisoire)",
        11 => "Règles métier (provisoire)",
        12 => "Intégrations (provisoire)",
        13 => "Non-fonctionnel (provisoire)",
        14 => "Priorisation MVP (provisoire)",
        15 => "Planning (provisoire)",
        16 => "Validation (provisoire)",
        17 => "Synthèse (provisoire)",
        18 => "Transfert prompt maître (provisoire)",
        _ => $"Phase {numero}"
    };
}
