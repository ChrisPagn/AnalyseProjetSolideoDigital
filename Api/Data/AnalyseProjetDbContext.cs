using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AnalyseProjetDbContext(
    DbContextOptions<AnalyseProjetDbContext> options,
    IUtilisateurCourantAccessor? utilisateurCourantAccessor = null)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Projet> Projets => Set<Projet>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<InformationRegistre> InformationsRegistre => Set<InformationRegistre>();
    public DbSet<QuestionRegistre> QuestionsRegistre => Set<QuestionRegistre>();
    public DbSet<RisqueRegistre> RisquesRegistre => Set<RisqueRegistre>();
    public DbSet<DecisionRegistre> DecisionsRegistre => Set<DecisionRegistre>();
    public DbSet<HistoriqueModification> HistoriqueModifications => Set<HistoriqueModification>();
    public DbSet<Probleme> Problemes => Set<Probleme>();
    public DbSet<Processus> Processus => Set<Processus>();
    public DbSet<EtapeProcessus> EtapesProcessus => Set<EtapeProcessus>();
    public DbSet<Acteur> Acteurs => Set<Acteur>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Entite> Entites => Set<Entite>();
    public DbSet<DocumentMetier> DocumentsMetier => Set<DocumentMetier>();
    public DbSet<Fonctionnalite> Fonctionnalites => Set<Fonctionnalite>();
    public DbSet<Automatisation> Automatisations => Set<Automatisation>();
    public DbSet<CritereAcceptation> CriteresAcceptation => Set<CritereAcceptation>();
    public DbSet<LienTracabilite> LiensTracabilite => Set<LienTracabilite>();
    public DbSet<CompteurCode> CompteursCode => Set<CompteurCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Client / Projet -------------------------------------------------
        builder.Entity<Client>(e =>
        {
            e.Property(c => c.Nom).IsRequired().HasMaxLength(200);
        });

        builder.Entity<Projet>(e =>
        {
            e.Property(p => p.Nom).IsRequired().HasMaxLength(200);
            e.Property(p => p.NiveauMaturite).HasConversion<string>().HasMaxLength(30);
            e.Property(p => p.Statut).HasConversion<string>().HasMaxLength(30);
            e.HasOne(p => p.Client)
                .WithMany(c => c.Projets)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Phase -------------------------------------------------------------
        builder.Entity<Phase>(e =>
        {
            e.Property(p => p.Nom).IsRequired().HasMaxLength(200);
            e.Property(p => p.Statut).HasConversion<string>().HasMaxLength(30);
            e.HasOne(p => p.Projet)
                .WithMany(pr => pr.Phases)
                .HasForeignKey(p => p.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.ProjetId, p.Numero }).IsUnique();
        });

        // --- Registres -----------------------------------------------------------
        builder.Entity<InformationRegistre>(e =>
        {
            e.Property(i => i.Code).IsRequired().HasMaxLength(20);
            e.Property(i => i.Libelle).IsRequired().HasMaxLength(300);
            e.Property(i => i.Source).HasConversion<string>().HasMaxLength(30);
            e.Property(i => i.Statut).HasConversion<string>().HasMaxLength(30);
            e.Property(i => i.EntiteType).HasConversion<string>().HasMaxLength(30);
            e.HasOne(i => i.Projet)
                .WithMany(p => p.Informations)
                .HasForeignKey(i => i.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Phase)
                .WithMany()
                .HasForeignKey(i => i.PhaseId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(i => new { i.ProjetId, i.Code }).IsUnique();
            e.HasIndex(i => new { i.EntiteType, i.EntiteReferenceId });

            // EntiteType et EntiteReferenceId sont renseignés ensemble ou pas du tout (lien
            // polymorphe optionnel, Option B — voir docs/03-proposition-phases-05-18-v2.md).
            e.ToTable(t => t.HasCheckConstraint(
                "CK_InformationRegistre_LienPolymorpheCoherent",
                "(\"EntiteType\" IS NULL AND \"EntiteReferenceId\" IS NULL) OR (\"EntiteType\" IS NOT NULL AND \"EntiteReferenceId\" IS NOT NULL)"));
        });

        builder.Entity<QuestionRegistre>(e =>
        {
            e.Property(q => q.Code).IsRequired().HasMaxLength(20);
            e.Property(q => q.Question).IsRequired().HasMaxLength(500);
            e.Property(q => q.Importance).HasConversion<string>().HasMaxLength(20);
            e.Property(q => q.Statut).HasConversion<string>().HasMaxLength(20);
            e.HasOne(q => q.Projet)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(q => q.Phase)
                .WithMany()
                .HasForeignKey(q => q.PhaseId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(q => new { q.ProjetId, q.Code }).IsUnique();
        });

        builder.Entity<RisqueRegistre>(e =>
        {
            e.Property(r => r.Code).IsRequired().HasMaxLength(20);
            e.Property(r => r.Description).IsRequired().HasMaxLength(500);
            e.HasOne(r => r.Projet)
                .WithMany(p => p.Risques)
                .HasForeignKey(r => r.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.ProjetId, r.Code }).IsUnique();
        });

        builder.Entity<DecisionRegistre>(e =>
        {
            e.Property(d => d.Code).IsRequired().HasMaxLength(20);
            e.Property(d => d.Description).IsRequired().HasMaxLength(500);
            e.HasOne(d => d.Projet)
                .WithMany(p => p.Decisions)
                .HasForeignKey(d => d.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(d => new { d.ProjetId, d.Code }).IsUnique();
        });

        builder.Entity<HistoriqueModification>(e =>
        {
            e.Property(h => h.EntiteType).IsRequired().HasMaxLength(100);
            e.Property(h => h.Champ).IsRequired().HasMaxLength(100);
            e.Property(h => h.ModifiePar).IsRequired().HasMaxLength(100);
            e.HasIndex(h => new { h.EntiteType, h.EntiteId });
        });

        // --- Domaine projet analysé ----------------------------------------------
        builder.Entity<Probleme>(e =>
        {
            e.Property(p => p.Code).IsRequired().HasMaxLength(20);
            e.Property(p => p.Description).IsRequired().HasMaxLength(500);
            e.Property(p => p.Gravite).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Frequence).HasColumnType("decimal(10,2)");
            e.Property(p => p.ImpactTempsHeuresMois).HasColumnType("decimal(10,2)");
            e.Property(p => p.CoutEstime).HasColumnType("decimal(10,2)");
            e.Property(p => p.ScoreCalcule).HasColumnType("decimal(12,2)");
            e.HasOne(p => p.Projet)
                .WithMany(pr => pr.Problemes)
                .HasForeignKey(p => p.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.ProjetId, p.Code }).IsUnique();
        });

        builder.Entity<Processus>(e =>
        {
            e.Property(p => p.Code).IsRequired().HasMaxLength(20);
            e.Property(p => p.Nom).IsRequired().HasMaxLength(200);
            e.HasOne(p => p.Projet)
                .WithMany(pr => pr.Processus)
                .HasForeignKey(p => p.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.ProjetId, p.Code }).IsUnique();
        });

        builder.Entity<EtapeProcessus>(e =>
        {
            e.Property(s => s.Acteur).IsRequired().HasMaxLength(200);
            e.Property(s => s.Action).IsRequired().HasMaxLength(300);
            e.HasOne(s => s.Processus)
                .WithMany(p => p.Etapes)
                .HasForeignKey(s => s.ProcessusId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Acteur>(e =>
        {
            e.Property(a => a.Code).IsRequired().HasMaxLength(20);
            e.Property(a => a.Nom).IsRequired().HasMaxLength(200);
            e.HasOne(a => a.Projet)
                .WithMany(p => p.Acteurs)
                .HasForeignKey(a => a.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.ProjetId, a.Code }).IsUnique();
        });

        builder.Entity<Permission>(e =>
        {
            e.Property(p => p.EntiteConcernee).IsRequired().HasMaxLength(200);
            e.HasOne(p => p.Acteur)
                .WithMany(a => a.Permissions)
                .HasForeignKey(p => p.ActeurId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Entite>(e =>
        {
            e.Property(en => en.Code).IsRequired().HasMaxLength(20);
            e.Property(en => en.Nom).IsRequired().HasMaxLength(200);
            e.HasOne(en => en.Projet)
                .WithMany(p => p.Entites)
                .HasForeignKey(en => en.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(en => new { en.ProjetId, en.Code }).IsUnique();
        });

        builder.Entity<DocumentMetier>(e =>
        {
            e.Property(d => d.Code).IsRequired().HasMaxLength(20);
            e.Property(d => d.Type).IsRequired().HasMaxLength(100);
            e.HasOne(d => d.Projet)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(d => new { d.ProjetId, d.Code }).IsUnique();
        });

        builder.Entity<Fonctionnalite>(e =>
        {
            e.Property(f => f.Code).IsRequired().HasMaxLength(20);
            e.Property(f => f.Nom).IsRequired().HasMaxLength(200);
            e.Property(f => f.Priorite).HasConversion<string>().HasMaxLength(20);
            e.Property(f => f.Statut).HasConversion<string>().HasMaxLength(30);
            e.HasOne(f => f.Projet)
                .WithMany(p => p.Fonctionnalites)
                .HasForeignKey(f => f.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Acteur)
                .WithMany(a => a.Fonctionnalites)
                .HasForeignKey(f => f.ActeurId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(f => new { f.ProjetId, f.Code }).IsUnique();
        });

        builder.Entity<Automatisation>(e =>
        {
            e.Property(a => a.Code).IsRequired().HasMaxLength(20);
            e.Property(a => a.Declencheur).IsRequired().HasMaxLength(300);
            e.Property(a => a.Action).IsRequired().HasMaxLength(300);
            e.HasOne(a => a.Projet)
                .WithMany(p => p.Automatisations)
                .HasForeignKey(a => a.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.ProjetId, a.Code }).IsUnique();
        });

        builder.Entity<CritereAcceptation>(e =>
        {
            e.Property(c => c.Given).IsRequired().HasMaxLength(500);
            e.Property(c => c.When).IsRequired().HasMaxLength(500);
            e.Property(c => c.Then).IsRequired().HasMaxLength(500);
            e.HasOne(c => c.Fonctionnalite)
                .WithMany(f => f.CriteresAcceptation)
                .HasForeignKey(c => c.FonctionnaliteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- LienTracabilite : source de vérité unique (Prompt Maître 4.1, 4.3, 14) ---
        builder.Entity<LienTracabilite>(e =>
        {
            e.HasOne(l => l.Probleme)
                .WithMany(p => p.LiensTracabilite)
                .HasForeignKey(l => l.ProblemeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.Fonctionnalite)
                .WithMany(f => f.LiensTracabilite)
                .HasForeignKey(l => l.FonctionnaliteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.Entite)
                .WithMany(en => en.LiensTracabilite)
                .HasForeignKey(l => l.EntiteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.CritereAcceptation)
                .WithMany()
                .HasForeignKey(l => l.CritereAcceptationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.InformationRegistre)
                .WithMany()
                .HasForeignKey(l => l.InformationRegistreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Au moins un des 5 liens doit être renseigné (Prompt Maître 4.1 : "liens optionnels,
            // au moins un requis" — étendu à InformationRegistreId pour couvrir les
            // fonctionnalités transversales justifiées par une Contrainte/Règle/Exigence NF,
            // voir docs/03-proposition-phases-05-18-v2.md). Portée en base, pas seulement en
            // validation applicative, pour qu'aucun appelant futur ne puisse créer une ligne
            // totalement orpheline.
            e.ToTable(t => t.HasCheckConstraint(
                "CK_LienTracabilite_AuMoinsUnLien",
                "\"ProblemeId\" IS NOT NULL OR \"FonctionnaliteId\" IS NOT NULL OR \"EntiteId\" IS NOT NULL OR \"CritereAcceptationId\" IS NOT NULL OR \"InformationRegistreId\" IS NOT NULL"));
        });

        // --- CompteurCode : génération des codes INF-xxx/Q-xxx/R-xxx/DEC-xxx (Prompt Maître 4.3) ---
        builder.Entity<CompteurCode>(e =>
        {
            e.Property(c => c.Prefixe).IsRequired().HasMaxLength(10);
            e.HasIndex(c => new { c.ProjetId, c.Prefixe }).IsUnique();
        });
    }

    /// <summary>
    /// Entités des 4 registres (Prompt Maître 4.1) dont chaque modification de champ scalaire est
    /// journalisée automatiquement dans HistoriqueModification — mécanisme générique choisi pour
    /// ne nécessiter aucun code supplémentaire par futur champ ou par future Action (section 5.5).
    /// </summary>
    private static readonly Type[] TypesJournalises =
    [
        typeof(InformationRegistre),
        typeof(QuestionRegistre),
        typeof(RisqueRegistre),
        typeof(DecisionRegistre)
    ];

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var entreesHistorique = CapturerModifications();
        var resultat = base.SaveChanges(acceptAllChangesOnSuccess);
        PersisterHistorique(entreesHistorique);
        return resultat;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entreesHistorique = CapturerModifications();
        var resultat = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await PersisterHistoriqueAsync(entreesHistorique, cancellationToken);
        return resultat;
    }

    private List<HistoriqueModification> CapturerModifications()
    {
        var entrees = new List<HistoriqueModification>();
        var modifiePar = utilisateurCourantAccessor?.ObtenirIdentifiant() ?? "système";
        var maintenant = DateTime.UtcNow;

        foreach (var entree in ChangeTracker.Entries().Where(
                     e => e.State == EntityState.Modified && TypesJournalises.Contains(e.Entity.GetType())))
        {
            var idProperty = entree.Property("Id");
            var entiteId = idProperty.CurrentValue is int id ? id : 0;

            foreach (var propriete in entree.Properties.Where(p => p.IsModified && p.Metadata.Name != "Id"))
            {
                var ancienneValeur = propriete.OriginalValue?.ToString();
                var nouvelleValeur = propriete.CurrentValue?.ToString();

                if (ancienneValeur == nouvelleValeur)
                {
                    continue;
                }

                entrees.Add(new HistoriqueModification
                {
                    EntiteType = entree.Entity.GetType().Name,
                    EntiteId = entiteId,
                    Champ = propriete.Metadata.Name,
                    AncienneValeur = ancienneValeur,
                    NouvelleValeur = nouvelleValeur,
                    ModifiePar = modifiePar,
                    DateModification = maintenant
                });
            }
        }

        return entrees;
    }

    private void PersisterHistorique(List<HistoriqueModification> entrees)
    {
        if (entrees.Count == 0)
        {
            return;
        }

        HistoriqueModifications.AddRange(entrees);
        base.SaveChanges(true);
    }

    private async Task PersisterHistoriqueAsync(List<HistoriqueModification> entrees, CancellationToken cancellationToken)
    {
        if (entrees.Count == 0)
        {
            return;
        }

        HistoriqueModifications.AddRange(entrees);
        await base.SaveChangesAsync(true, cancellationToken);
    }
}
