using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Secteur = table.Column<string>(type: "TEXT", nullable: true),
                    Taille = table.Column<string>(type: "TEXT", nullable: true),
                    Contact = table.Column<string>(type: "TEXT", nullable: true),
                    Adresse = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntiteType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Champ = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AncienneValeur = table.Column<string>(type: "TEXT", nullable: true),
                    NouvelleValeur = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiePar = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateModification = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueModifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NiveauMaturite = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    StackEnvisagee = table.Column<string>(type: "TEXT", nullable: true),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Acteurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Fonction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acteurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acteurs_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Automatisations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Declencheur = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Condition = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ValidationHumaine = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automatisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automatisations_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DecisionsRegistre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Justification = table.Column<string>(type: "TEXT", nullable: true),
                    AlternativesEcartees = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionsRegistre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionsRegistre_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentsMetier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Origine = table.Column<string>(type: "TEXT", nullable: true),
                    Destination = table.Column<string>(type: "TEXT", nullable: true),
                    Format = table.Column<string>(type: "TEXT", nullable: true),
                    DureeConservation = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsMetier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentsMetier_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Attributs = table.Column<string>(type: "TEXT", nullable: true),
                    Relations = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entites_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DateMaj = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phases_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Problemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Gravite = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Frequence = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ImpactTempsHeuresMois = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CoutEstime = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ScoreCalcule = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problemes_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Processus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Declencheur = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processus_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RisquesRegistre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Probabilite = table.Column<string>(type: "TEXT", nullable: true),
                    Impact = table.Column<string>(type: "TEXT", nullable: true),
                    Mesure = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RisquesRegistre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RisquesRegistre_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fonctionnalites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActeurId = table.Column<int>(type: "INTEGER", nullable: true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Priorite = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonctionnalites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fonctionnalites_Acteurs_ActeurId",
                        column: x => x.ActeurId,
                        principalTable: "Acteurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Fonctionnalites_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActeurId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntiteConcernee = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PeutVoir = table.Column<bool>(type: "INTEGER", nullable: false),
                    PeutCreer = table.Column<bool>(type: "INTEGER", nullable: false),
                    PeutModifier = table.Column<bool>(type: "INTEGER", nullable: false),
                    PeutSupprimer = table.Column<bool>(type: "INTEGER", nullable: false),
                    PeutValider = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Acteurs_ActeurId",
                        column: x => x.ActeurId,
                        principalTable: "Acteurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InformationsRegistre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PhaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Libelle = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Valeur = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationsRegistre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InformationsRegistre_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InformationsRegistre_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionsRegistre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PhaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Question = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Importance = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionsRegistre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionsRegistre_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuestionsRegistre_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtapesProcessus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcessusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ordre = table.Column<int>(type: "INTEGER", nullable: false),
                    Acteur = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Outil = table.Column<string>(type: "TEXT", nullable: true),
                    DureeEstimee = table.Column<string>(type: "TEXT", nullable: true),
                    ErreursConnues = table.Column<string>(type: "TEXT", nullable: true),
                    ExempleValide = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExempleDescription = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapesProcessus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtapesProcessus_Processus_ProcessusId",
                        column: x => x.ProcessusId,
                        principalTable: "Processus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteresAcceptation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FonctionnaliteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Given = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    When = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Then = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteresAcceptation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteresAcceptation_Fonctionnalites_FonctionnaliteId",
                        column: x => x.FonctionnaliteId,
                        principalTable: "Fonctionnalites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiensTracabilite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    FonctionnaliteId = table.Column<int>(type: "INTEGER", nullable: true),
                    EntiteId = table.Column<int>(type: "INTEGER", nullable: true),
                    CritereAcceptationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiensTracabilite", x => x.Id);
                    table.CheckConstraint("CK_LienTracabilite_AuMoinsUnLien", "\"ProblemeId\" IS NOT NULL OR \"FonctionnaliteId\" IS NOT NULL OR \"EntiteId\" IS NOT NULL OR \"CritereAcceptationId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_LiensTracabilite_CriteresAcceptation_CritereAcceptationId",
                        column: x => x.CritereAcceptationId,
                        principalTable: "CriteresAcceptation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiensTracabilite_Entites_EntiteId",
                        column: x => x.EntiteId,
                        principalTable: "Entites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiensTracabilite_Fonctionnalites_FonctionnaliteId",
                        column: x => x.FonctionnaliteId,
                        principalTable: "Fonctionnalites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiensTracabilite_Problemes_ProblemeId",
                        column: x => x.ProblemeId,
                        principalTable: "Problemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acteurs_ProjetId_Code",
                table: "Acteurs",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Automatisations_ProjetId_Code",
                table: "Automatisations",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteresAcceptation_FonctionnaliteId",
                table: "CriteresAcceptation",
                column: "FonctionnaliteId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionsRegistre_ProjetId_Code",
                table: "DecisionsRegistre",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsMetier_ProjetId_Code",
                table: "DocumentsMetier",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entites_ProjetId_Code",
                table: "Entites",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapesProcessus_ProcessusId",
                table: "EtapesProcessus",
                column: "ProcessusId");

            migrationBuilder.CreateIndex(
                name: "IX_Fonctionnalites_ActeurId",
                table: "Fonctionnalites",
                column: "ActeurId");

            migrationBuilder.CreateIndex(
                name: "IX_Fonctionnalites_ProjetId_Code",
                table: "Fonctionnalites",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueModifications_EntiteType_EntiteId",
                table: "HistoriqueModifications",
                columns: new[] { "EntiteType", "EntiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_InformationsRegistre_PhaseId",
                table: "InformationsRegistre",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_InformationsRegistre_ProjetId_Code",
                table: "InformationsRegistre",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiensTracabilite_CritereAcceptationId",
                table: "LiensTracabilite",
                column: "CritereAcceptationId");

            migrationBuilder.CreateIndex(
                name: "IX_LiensTracabilite_EntiteId",
                table: "LiensTracabilite",
                column: "EntiteId");

            migrationBuilder.CreateIndex(
                name: "IX_LiensTracabilite_FonctionnaliteId",
                table: "LiensTracabilite",
                column: "FonctionnaliteId");

            migrationBuilder.CreateIndex(
                name: "IX_LiensTracabilite_ProblemeId",
                table: "LiensTracabilite",
                column: "ProblemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ActeurId",
                table: "Permissions",
                column: "ActeurId");

            migrationBuilder.CreateIndex(
                name: "IX_Phases_ProjetId_Numero",
                table: "Phases",
                columns: new[] { "ProjetId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problemes_ProjetId_Code",
                table: "Problemes",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processus_ProjetId_Code",
                table: "Processus",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projets_ClientId",
                table: "Projets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionsRegistre_PhaseId",
                table: "QuestionsRegistre",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionsRegistre_ProjetId_Code",
                table: "QuestionsRegistre",
                columns: new[] { "ProjetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RisquesRegistre_ProjetId_Code",
                table: "RisquesRegistre",
                columns: new[] { "ProjetId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Automatisations");

            migrationBuilder.DropTable(
                name: "DecisionsRegistre");

            migrationBuilder.DropTable(
                name: "DocumentsMetier");

            migrationBuilder.DropTable(
                name: "EtapesProcessus");

            migrationBuilder.DropTable(
                name: "HistoriqueModifications");

            migrationBuilder.DropTable(
                name: "InformationsRegistre");

            migrationBuilder.DropTable(
                name: "LiensTracabilite");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "QuestionsRegistre");

            migrationBuilder.DropTable(
                name: "RisquesRegistre");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Processus");

            migrationBuilder.DropTable(
                name: "CriteresAcceptation");

            migrationBuilder.DropTable(
                name: "Entites");

            migrationBuilder.DropTable(
                name: "Problemes");

            migrationBuilder.DropTable(
                name: "Phases");

            migrationBuilder.DropTable(
                name: "Fonctionnalites");

            migrationBuilder.DropTable(
                name: "Acteurs");

            migrationBuilder.DropTable(
                name: "Projets");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
