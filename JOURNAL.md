# Journal de développement — AnalyseProjetSolideoDigital

## Étape 1 — Fondations (2026-09-18)

### Contenu réalisé
- Phase Architecte (section 0.1) complétée : cohérence sections 4/5/9/10 validée, deux points
  tranchés avec l'utilisateur (Caddyfile → exposition publique DuckDNS ; infra déjà opérationnelle
  sur le home server).
- Dépôt git initialisé (`git init`), documents maîtres déplacés dans `docs/`.
- Solution .NET 9 à 4 projets : `Client` (Blazor WASM autonome), `Api` (ASP.NET Core Web API,
  controllers), `Shared` (classlib), `Api.Tests` (xUnit).
- Structure de dossiers conforme à la section 8 : `Api/Services`, `Api/Actions`, `Api/Data`,
  `Api/Data/Migrations`, `Shared/Dtos`, `Shared/Enums`, `Client/Pages/{Clients,Projets,Phases}`,
  `Client/Components`, `Client/Services`.
- Toutes les entités du modèle de domaine (Prompt Maître 4.1) créées dans `Api/Data/Entities` :
  Client, Projet, Phase, InformationRegistre, QuestionRegistre, RisqueRegistre, DecisionRegistre,
  HistoriqueModification, Probleme, Processus, EtapeProcessus, Acteur, Permission, Entite,
  DocumentMetier, Fonctionnalite, Automatisation, CritereAcceptation, LienTracabilite, plus
  `ApplicationUser` (Identity).
- Enums métier dans `Shared/Enums` : SourceInformation, StatutInformation, Gravite,
  NiveauMaturite, StatutPhase, StatutProjet, ImportanceQuestion, StatutQuestion, PrioriteMoSCoW,
  StatutFonctionnalite.
- `AnalyseProjetDbContext` (hérite de `IdentityDbContext<ApplicationUser>`) : tous les enums
  configurés en `HasConversion<string>()` (pas d'ENUM SQL natif, interdiction 14), index uniques
  par projet sur chaque code de registre (INF-xxx, PROB-xxx, etc.), contrainte CHECK SQL sur
  `LienTracabilite` imposant qu'au moins un des 4 liens soit renseigné (portée en base, pas
  seulement en validation applicative).
- Migration EF Core initiale `InitialCreate` générée et appliquée sur SQLite dev.
- `Program.cs` : sélection du provider EF Core (Sqlite/MySQL Pomelo) via configuration
  (`DatabaseProvider`), jamais hardcodé ; Identity Core avec politique de mot de passe renforcée
  (12 caractères min., majuscule + chiffre + caractère spécial requis — posture sécurité "outil
  exposé publiquement", section 7.5) ; Client Blazor WASM servi comme fichiers statiques par le
  même conteneur Api (`UseBlazorFrameworkFiles` + fallback `index.html`) ; seed automatique en
  environnement Development uniquement.
- `DbSeeder` : 3 projets fictifs à des stades de maturité différents (Niveau 0, 1, 2), avec
  processus/étapes, problème quantifié + fonctionnalité + lien de traçabilité pour le projet le
  plus avancé.
- Docker : `Dockerfile` multi-stage (publie Client puis Api, copie le wwwroot du Client dans le
  conteneur final), `docker-compose.yml` (service unique `app` + `caddy`, conforme à la section
  7.3), `Caddyfile` corrigé pour l'exposition publique DuckDNS (le texte résiduel mentionnant
  Tailscale, contradictoire avec la section 7.5, a été supprimé), `.env.example`.
- Tests xUnit (`Api.Tests/Data/AnalyseProjetDbContextTests.cs`) : création du schéma, rejet d'un
  `LienTracabilite` sans aucun lien (contrainte CHECK), acceptation d'un lien valide, valeur par
  défaut de `NiveauMaturite`. 4/4 tests passent. Utilise SQLite en mémoire (pas le provider
  InMemory d'EF Core) car les contraintes CHECK ne sont évaluées que par un vrai provider
  relationnel.
- `.gitignore` : exclut `bin/`, `obj/`, les bases SQLite locales (`Api/data/`, `*.db`), le volume
  Docker `data/`.

### Décisions d'architecture prises
- Toutes les entités du modèle 4.1 sont créées dès l'étape 1 (une seule migration de structure),
  plutôt qu'un socle minimal enrichi progressivement — décision validée avec l'utilisateur.
- Le Client Blazor WASM est hébergé dans le même conteneur Docker que l'Api (fichiers statiques
  servis par ASP.NET Core), pas de service Docker séparé — décision validée avec l'utilisateur,
  cohérente avec le docker-compose.yml de référence (section 7.3, un seul service `app`).
- Versions des packages EF Core épinglées explicitement à `9.0.9` (et Pomelo à `9.0.0`) : au
  moment du développement, EF Core 10.0.12 est remonté comme version "latest" sur NuGet mais
  n'est pas compatible net9.0 — épingler évite que de futures commandes `dotnet add package`
  sans version explicite ne cassent la compatibilité de framework.

### Problèmes connus / points ouverts
- Les noms des phases 5 à 18 utilisés dans `DbSeeder` sont **provisoires** (marqués
  "(provisoire)" dans le code) : seul le Bloc A (phases 1-4) est détaillé dans le Guide des 18
  phases fourni. À corriger dès que les Blocs B à E seront rédigés.
- Les seuils `NiveauParPhases` pour les paliers 3 et 4 (Blocs B/C/D) ne sont pas encore fixés
  dans le Prompt Maître (section 4.3) — `MaturiteCalculatorService` ne sera implémenté qu'à partir
  de l'étape 3, en s'en tenant au Bloc A tant que ces seuils ne sont pas communiqués.
- Rate limiting sur l'authentification : le service `AddRateLimiter` est enregistré en socle dans
  `Program.cs` mais sans règles concrètes — l'implémentation complète (politique de limite sur
  l'endpoint de login) est prévue à l'étape 2 (Auth minimale), pas encore livrée.
- 2FA et CrowdSec (section 7.5) restent hors périmètre du code applicatif V1, à traiter au niveau
  infra par l'utilisateur avant mise en production réelle — rappel non bloquant pour la suite du
  développement.

### Fichiers créés/modifiés
Voir `git log` / `git status` — première validation de l'étape, pas encore de commit créé (en
attente de validation de l'étape par l'utilisateur avant commit, conformément au mode strict).

## Étape 2 — Auth minimale (2026-09-18)

### Contenu réalisé
- Mécanisme d'authentification tranché avec l'utilisateur : cookie ASP.NET Core Identity
  (HttpOnly), pas de JWT — cohérent avec le déploiement mono-conteneur (Client et Api sur la
  même origine en production, Prompt Maître 7.3).
- `Shared/Dtos/Auth` : `LoginRequestDto`, `CurrentUserDto`.
- `Api/Validators/LoginRequestDtoValidator` (FluentValidation, section 5.3) : e-mail et mot de
  passe requis, format e-mail validé côté serveur.
- `Api/Services/AuthService` : logique de connexion/déconnexion isolée des Controllers (section
  5.1) — vérification de mot de passe factice si l'e-mail n'existe pas, pour ne pas révéler par
  timing si un compte existe.
- `Api/Controllers/AuthController` : `POST /api/auth/login` (rate limité), `POST /api/auth/logout`
  (authentifié), `GET /api/auth/me` — orchestration uniquement, pas de logique métier.
- `Api/Data/AdminSeeder` : crée le compte admin unique (section 9) au démarrage à partir de la
  configuration (`AdminAccount:Email` / `AdminAccount:Password` — variables d'environnement en
  prod, user-secrets en dev), jamais de mot de passe hardcodé (interdiction absolue, section 14).
- `Program.cs` : politique de mot de passe renforcée + `Lockout` (5 tentatives, 15 min) sur le
  compte Identity ; rate limiting HTTP dédié (`[EnableRateLimiting("login")]`, 5 requêtes/minute
  par IP, 429 au-delà) sur l'endpoint de login — les deux mécanismes se complètent (section 7.5) ;
  `ForwardedHeaders` pour que l'Api reconnaisse le HTTPS terminé par Caddy en amont
  (`CookieSecurePolicy.SameAsRequest`) ; CORS + cookie `SameSite=Lax` activés **uniquement** en
  environnement Development (Client et Api sur des ports séparés en dev local) — `SameSite=Strict`
  en production où tout passe par la même origine.
- Client Blazor : `CookieAuthenticationStateProvider` (interroge `/api/auth/me`), `AuthClient`
  (wrapper HttpClient typé avec `BrowserRequestCredentials.Include` pour que le cookie voyage en
  dev cross-origin), page `Login.razor` (MudBlazor, responsive, touch target ≥ 44px),
  `MainLayout.razor` reconstruit avec `MudLayout`/`MudAppBar`/`MudDrawer` repliable affiché
  seulement si authentifié, `RedirectToLogin` + `AuthorizeRouteView` dans `App.razor`, page
  `Index.razor` protégée par `[Authorize]`.
- MudBlazor câblé pour de vrai (services, CSS/JS, thème) — posé à l'étape 1 mais jamais utilisé
  jusqu'ici.
- Tests xUnit (`Api.Tests/Controllers/AuthControllerTests.cs` et `AuthRateLimitingTests.cs`) :
  `/me` anonyme, login valide pose un cookie, mauvais mot de passe / e-mail inexistant → 401 sans
  distinction, login puis `/me` authentifié, logout sans session → 401, DTO invalide → 400,
  rate limiting → 429 après 5 requêtes/minute. 12/12 tests passent au total (4 hérités de
  l'étape 1 + 8 nouveaux). `AnalyseProjetWebApplicationFactory` dédiée : base SQLite en mémoire,
  identifiants admin de test injectés via configuration.

### Décisions d'architecture prises
- Cookie Identity plutôt que JWT — décision validée avec l'utilisateur, la plus simple et la plus
  sûre par défaut vu que Client et Api partagent la même origine en production.
- `CookieSecurePolicy.SameAsRequest` (pas `Always`) : nécessaire pour que le cookie fonctionne en
  dev local HTTP et dans `WebApplicationFactory` (HTTP simple sans TLS), tout en restant marqué
  `Secure` en production dès que la requête est HTTPS (via `ForwardedHeaders`, Caddy terminant le
  TLS en amont).
- CORS + `SameSite=Lax` uniquement en environnement Development : nécessaire car le
  BlazorWebAssembly DevServer standalone (template `blazorwasm --empty`, pas le gabarit
  "Hosted") ne propose **pas** de mécanisme de proxy `/api/*` fonctionnel en .NET 9 contrairement
  à ce qui a été tenté initialement (`--proxy-config-file` : cette option n'existe pas pour ce
  gabarit et a été abandonnée après vérification directe de la documentation Microsoft). En
  production, ce contournement est inactif (même origine, `SameSite=Strict`) — aucun compromis de
  sécurité en prod.
- `wwwroot/appsettings.Development.json` (Client) fixe `ApiBaseUrl` vers `http://localhost:5118/`
  pour le dev local ; absent en production, où le Client retombe sur l'origine courante.

### Problèmes connus / points ouverts
- Aucun nouveau point ouvert. Les points de l'étape 1 (noms de phases 5-18 provisoires, seuils
  `NiveauParPhases` des Blocs B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.
- Testé manuellement de bout en bout dans un vrai navigateur (Chrome headless piloté via Chrome
  DevTools Protocol) : page de connexion (desktop et mobile 375px), saisie, connexion réussie,
  redirection vers le tableau de bord authentifié avec AppBar/drawer/e-mail affiché, et validation
  que la page de connexion ne montre aucune UI authentifiée tant que la session n'existe pas.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 3 — Clients & Projets (2026-09-18)

### Contenu réalisé
- `Shared/Dtos/Clients` : `ClientDto`, `UpsertClientDto`. `Shared/Dtos/Projets` : `ProjetDto`
  (inclut `NiveauMaturite` en lecture seule), `UpsertProjetDto` (aucun champ `NiveauMaturite` —
  jamais assignable directement, interdiction absolue section 14).
- `Api/Validators` : `UpsertClientDtoValidator`, `UpsertProjetDtoValidator` (FluentValidation,
  section 5.3).
- `Api/Services/MaturiteCalculatorService` : implémente la formule complète du Prompt Maître 4.3
  dès cette étape (décision validée avec l'utilisateur, plutôt qu'un stub) —
  `NiveauMaturite = MIN(NiveauParPhases, NiveauMaxAutorise)`. `NiveauParPhases` couvre le Bloc A
  figé (paliers 0/1/2) et le cas Niveau 5 (18 phases terminées) ; plafonné à 2 tant que les seuils
  des Blocs B-E ne sont pas fixés dans le Prompt Maître (point ouvert documenté dans le code).
  `NiveauMaxAutorise` lit réellement `QuestionRegistre` (bloquante+ouverte → 1),
  `Fonctionnalite`/`Probleme` sans `LienTracabilite` (→ 4), sinon 5 — toutes ces tables existent
  depuis l'étape 1, donc le calcul est correct dès maintenant même si aucune UI ne les remplit
  encore (arrivera aux étapes 5/6/8).
- `Api/Actions/CreateProjetAction` : création d'un Projet dans une transaction EF Core explicite
  (section 5.4) — initialise les 18 Phases (`NonCommencee`) puis calcule le niveau de maturité
  initial (0) avant de committer.
- `Api/Actions/DeleteClientAction` : refuse la suppression d'un Client ayant des Projets rattachés
  avec un message métier clair (409 Conflict), plutôt que de laisser remonter l'exception SQL de
  contrainte de clé étrangère (`DeleteBehavior.Restrict`, posé à l'étape 1).
- `Api/Controllers/ClientsController` et `ProjetsController` (protégés `[Authorize]`) : CRUD
  complet + `POST /api/projets/{id}/recalculer-maturite`.
- Client Blazor : `ClientsApiClient`, `ProjetsApiClient` (wrappers HttpClient typés, section 5.1),
  pages `Clients.razor`/`Projets.razor` (tables MudBlazor responsive) + dialogs
  `ClientFormDialog.razor`/`ProjetFormDialog.razor` pour créer/modifier, confirmation de
  suppression via `MudMessageBox`. Chip coloré affichant le niveau de maturité par projet.
- Tests xUnit : `MaturiteCalculatorServiceTests` couvre tous les paliers de la formule 4.3 listés
  en section 12 (0/1/2/5 phases, question bloquante ouverte/résolue, fonctionnalité orpheline,
  problème non couvert, lien de traçabilité valide) + persistance. `ClientsControllerTests` et
  `ProjetsControllerTests` : CRUD complet, refus de suppression d'un client avec projet rattaché,
  refus de création avec client inexistant, DTO invalide → 400. 35/35 tests passent au total
  (12 hérités des étapes 1-2 + 23 nouveaux).
- Flux CRUD complet vérifié dans un vrai navigateur (Chrome headless via CDP) : création de client
  et de projet via les dialogs, rafraîchissement de la liste, affichage correct des chips de
  maturité pour les projets du seeder (0, 0, 1, 2 selon leur avancement).

### Décisions d'architecture prises
- `MaturiteCalculatorService` complet dès l'étape 3 plutôt qu'un stub limité au Bloc A — décision
  validée avec l'utilisateur : le service interroge directement les tables déjà en base, donc le
  calcul est correct dès maintenant et se remplira naturellement aux étapes suivantes sans qu'il
  faille revenir modifier ce service à chaque fois.
- Existence du `ClientId` sur `UpsertProjetDto` vérifiée dans le Controller plutôt que dans le
  Validator FluentValidation : `AddFluentValidationAutoValidation()` (posé à l'étape 2) utilise le
  pipeline de validation MVC **synchrone**, qui ne supporte pas les règles asynchrones
  (`MustAsync`) — une première tentative avec `MustAsync` provoquait une exception
  `AsyncValidatorInvokedSynchronouslyException` non gérée (500 sur **chaque** création de projet).
  Détecté par les tests d'intégration avant que ça n'atteigne la production ; corrigé en gardant
  la validation FluentValidation synchrone (formats, longueurs) et en déplaçant la vérification
  d'existence du client dans le Controller (`ModelState.AddModelError` + `ValidationProblem`).

### Problèmes connus / points ouverts
- Aucun nouveau point ouvert. Les points des étapes 1-2 (noms de phases 5-18 provisoires, seuils
  `NiveauParPhases` des Blocs B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables
  — `NiveauMaxAutorise`, lui, est désormais pleinement fonctionnel.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 4 — Phases & navigation (2026-09-18)

### Contenu réalisé
- Décision validée avec l'utilisateur : navigation libre (non bloquante) entre les 18 phases —
  l'utilisateur peut cliquer directement sur n'importe quelle phase à tout moment, cohérent avec
  l'usage réel en RDV (Guide 18 phases : une info détectée en Phase 03 doit pouvoir alimenter la
  Phase 10 immédiatement). Périmètre de l'étape limité à la navigation/statuts — le contenu métier
  de chaque phase (questions, informations à saisir) arrive aux étapes 5 (Registres) et 6 (Domaine
  projet analysé), comme prévu par l'ordre des modules (section 10).
- `Shared/Dtos/Phases` : `PhaseDto`, `UpdateStatutPhaseDto`.
- `Api/Actions/UpdateStatutPhaseAction` : changement de statut d'une Phase dans une transaction EF
  Core explicite (section 5.4) qui recalcule et persiste `NiveauMaturite` dans la foulée.
- `Api/Controllers/PhasesController` (`[Authorize]`, routes imbriquées sous `/api/projets/{projetId}/phases`) :
  liste des 18 phases d'un projet, détail par numéro, modification de statut (retourne le
  `ProjetDto` à jour pour rafraîchir l'affichage de la maturité sans requête supplémentaire).
- Client Blazor : `PhasesApiClient` (wrapper HttpClient typé), page
  `Client/Pages/Phases/PhasesProjet.razor` (route `/projets/{ProjetId}/phases/{Numero?}`) — liste
  des 18 phases avec icône de statut, panneau de détail avec boutons Non commencée/En cours/Terminée,
  navigation Phase précédente/suivante, affichage du niveau de maturité du projet mis à jour en
  direct après chaque changement de statut. Bouton d'accès depuis la page Projets.
- Tests xUnit (`PhasesControllerTests`) : les 18 phases sont créées dans le bon ordre, phase
  introuvable → 404, et surtout l'impact réel du changement de statut sur `NiveauMaturite`
  (Phases 01-02 terminées → Niveau 1 ; repasser une phase à Non commencée fait redescendre le
  niveau). 41/41 tests passent au total (35 hérités des étapes 1-3 + 6 nouveaux).
- Flux vérifié dans un vrai navigateur : navigation entre phases, changement de statut avec
  confirmation visuelle (snackbar + icônes + chip de maturité mis à jour en direct dans l'en-tête).

### Décisions d'architecture prises
- Navigation séquentielle non bloquante — décision validée avec l'utilisateur (voir ci-dessus).
- Route `/projets/{ProjetId}/phases/{Numero?}` avec `Numero` optionnel : à l'arrivée sans numéro,
  la page sélectionne automatiquement la première phase non terminée (comportement utile pour
  reprendre une analyse là où elle s'était arrêtée), plutôt que de forcer un choix explicite.

### Problèmes connus / points ouverts
- **Bug réel détecté et corrigé** : dans `UpdateStatutPhaseAction`, le nouveau statut de la Phase
  était assigné en mémoire (trackée par EF Core) mais `MaturiteCalculatorService.CalculerAsync`
  relit les Phases via une requête SQL directe (`db.Phases.Where(...).ToListAsync`), qui ne voyait
  donc pas encore le changement non persisté — le niveau de maturité calculé restait basé sur
  l'ancien statut. Corrigé en forçant un `SaveChangesAsync()` du changement de statut avant
  d'appeler le calculateur, dans la même transaction. Détecté immédiatement par les tests
  d'intégration (`ModifierStatut_phases_01_02_terminees_fait_passer_le_projet_au_niveau_1` échouait
  avant correction) — aucun risque que ce bug ait atteint un usage réel.
- Les points des étapes 1-3 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 5 — Registres (2026-09-18)

### Contenu réalisé
- Trois décisions de conception validées avec l'utilisateur avant de coder :
  1. Génération des codes (INF-xxx, Q-xxx, R-xxx, DEC-xxx) via un compteur dédié par
     projet+préfixe (`CompteurCode`), jamais par calcul MAX+1 — un numéro n'est donc jamais
     réutilisé après suppression d'une ligne.
  2. Journalisation de l'historique **générique**, via interception de
     `AnalyseProjetDbContext.SaveChanges(Async)` (change tracking EF Core), plutôt que du code
     manuel par Action — couvre déjà tout champ scalaire modifié sur les 4 registres, y compris
     pour des champs ou Actions futurs.
  3. Rattachement `PhaseId` "déduit du contexte + modifiable" : pré-rempli avec la phase consultée
     à la création, mais reste un select modifiable dans le formulaire (et vue transverse possible
     via le paramètre `?phaseId=` optionnel sur les endpoints GET).
- `Api/Data/Entities/CompteurCode` + migration `AddCompteurCode` ;
  `Api/Services/CodeSequenceService` (génération atomique des codes).
- `AnalyseProjetDbContext` : override de `SaveChanges`/`SaveChangesAsync` qui capture, avant
  persistance, toute propriété scalaire modifiée sur `InformationRegistre`, `QuestionRegistre`,
  `RisqueRegistre`, `DecisionRegistre` (liste `TypesJournalises`) et écrit une ligne
  `HistoriqueModification` par champ réellement changé (ignore les no-op). `ModifiePar` vient d'un
  nouveau `IUtilisateurCourantAccessor` (implémentation HTTP basée sur le claim e-mail), injecté en
  paramètre optionnel du DbContext pour ne pas casser les tests qui l'instancient directement.
- `Shared/Dtos/Registres` : DTOs des 4 registres + `HistoriqueModificationDto`.
- `Api/Validators` : un validator FluentValidation par registre (formats/longueurs uniquement —
  leçon de l'étape 3 : pas de règle asynchrone dans ces validators).
- Controllers `InformationsRegistreController`, `QuestionsRegistreController` (recalcule et
  persiste `NiveauMaturite` après chaque création/modification/suppression, une question bloquante
  pouvant plafonner le niveau — Prompt Maître 4.3), `RisquesRegistreController`,
  `DecisionsRegistreController` (tous imbriqués sous `/api/projets/{projetId}/...`, `[Authorize]`),
  et `HistoriqueController` (lecture seule — aucun endpoint d'écriture, alimenté uniquement par le
  DbContext).
- Client Blazor : `RegistresApiClient`, composant `RegistresPhase.razor` (onglets MudBlazor
  Informations/Questions/Risques/Décisions) intégré directement dans `PhasesProjet.razor` sous le
  panneau de détail de la phase active, avec ses 4 sous-composants dédiés
  (`RegistreInformations`, `RegistreQuestions`, `RegistreRisques`, `RegistreDecisions`) et leurs
  dialogs de formulaire. Le chip de maturité en en-tête se rafraîchit automatiquement après tout
  changement touchant `QuestionRegistre` (callback `SurChangement`).
- Tests xUnit : `CodeSequenceServiceTests` (codes successifs, compteurs indépendants par
  projet/préfixe, non-réutilisation après suppression), `HistoriqueModificationTests` (le
  mécanisme générique capture les bons champs, ignore les no-op et les suppressions, ignore les
  entités hors périmètre comme `Projet`), `RegistresControllerTests` (CRUD des 4 registres via
  HTTP, génération de codes réels, effet d'une question bloquante sur la maturité via l'API,
  historique consultable après une vraie modification HTTP). 62/62 tests passent au total
  (41 hérités des étapes 1-4 + 21 nouveaux).
- Flux vérifié dans un vrai navigateur : navigation vers une phase, ajout d'une information
  (code `INF-001` généré), ajout d'une question, marquage résolue, suppression — tout avec
  confirmation visuelle et rafraîchissement correct des listes.

### Décisions d'architecture prises
- Voir les 3 décisions validées avec l'utilisateur en tête de section.
- `IUtilisateurCourantAccessor` en paramètre optionnel (nullable) du constructeur du DbContext,
  plutôt qu'obligatoire : permet aux tests existants (`new AnalyseProjetDbContext(options)`, sans
  DI complète) de continuer à fonctionner sans modification, tout en activant la capture de
  `ModifiePar` quand le service est disponible (production, tests dédiés à l'historique).

### Problèmes connus / points ouverts
- Aucun nouveau bug applicatif détecté cette fois — la journalisation générique et la génération
  de codes ont fonctionné correctement dès la première implémentation, tests inclus.
- Les points des étapes 1-4 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 6 — Domaine projet analysé (2026-09-18)

### Contenu réalisé
- Trois décisions de conception validées avec l'utilisateur avant de coder : (1) `ScoreCalcule`
  d'un Problème calculé côté serveur uniquement (jamais saisi/assignable via l'UI, même principe
  que `NiveauMaturite`) ; (2) `Acteur`/`Permission` (grille Voir/Créer/Modifier/Supprimer/Valider
  par `EntiteConcernee`) inclus dès cette étape, pas reporté ; (3) les 7 entités sans `PhaseId`
  (Problème, Processus, Acteur, Entité, DocumentMetier, Fonctionnalité, Automatisation) vivent
  dans une **nouvelle page dédiée par projet** (`/projets/{id}/domaine`), distincte de la page
  Phase — cohérent avec le modèle de données où elles ne sont rattachées qu'au Projet.
- `Shared/Dtos/Domaine` : DTOs des 8 entités (y compris `EtapeProcessus`, `Permission`,
  `CritereAcceptation`, `LienTracabilite` en sous-ressources).
- `Api/Validators` : un validator FluentValidation par entité (formats/longueurs uniquement,
  toujours synchrone — leçon de l'étape 3).
- 8 Controllers imbriqués sous `/api/projets/{projetId}/...` (`[Authorize]`) :
  `ProblemesController` (calcule `ScoreCalcule` côté serveur, recalcule la maturité),
  `ProcessusController` (+ sous-ressource `etapes`), `ActeursController` (+ sous-ressource
  `permissions`), `EntitesController`, `DocumentsMetierController`, `FonctionnalitesController`
  (indicateur `EstOrpheline`, + sous-ressource `criteres`, recalcule la maturité),
  `AutomatisationsController`, `LiensTracabiliteController` (source de vérité unique pour la
  couverture besoin↔fonctionnalité, contrainte CHECK déjà en base depuis l'étape 1, recalcule la
  maturité à la création et à la suppression).
- Client Blazor : `DomaineApiClient`, page `DomaineProjet.razor` avec 8 onglets MudBlazor
  (Problèmes, Processus, Acteurs, Entités, Documents, Fonctionnalités, Automatisations,
  Traçabilité), chacun avec ses composants liste/dialog dédiés. Affichage en direct du score
  calculé pendant la saisie d'un Problème, badge « Orpheline » sur les Fonctionnalités non
  couvertes, coche « Couvert » sur les Problèmes reliés, gestion imbriquée des étapes de processus
  et des permissions par acteur. Bouton d'accès ajouté sur la page Projets.
- Tests xUnit (`DomaineControllerTests`, 18 tests) : génération de codes (PROB/PROC/ACT/ENT/
  DOC/AUTO/F-xxx), calcul du score, étapes de processus avec `ExempleValide`, permissions
  imbriquées, indicateur orpheline, impact réel sur `NiveauMaturite` (Problème non couvert et
  Fonctionnalité orpheline plafonnent à 4, un lien de traçabilité débloque le niveau 5, sa
  suppression fait redescendre la couverture), rejet d'un lien totalement vide. 77/77 tests
  passent au total (62 hérités des étapes 1-5 + 15 nouveaux, net des deux corrections ci-dessous).
- Flux vérifié dans un vrai navigateur : création de Problème avec score affiché en direct,
  création de Fonctionnalité orpheline, création d'un lien de traçabilité, et vérification
  visuelle que l'indicateur « Couvert » du Problème passe au vert après le lien.

### Décisions d'architecture prises
- Voir les 3 décisions validées avec l'utilisateur en tête de section.

### Problèmes connus / points ouverts — deux bugs réels détectés et corrigés
- **Bug de classe récurrente (recalcul avant persistance)** : `ProblemesController.Creer`,
  `FonctionnalitesController.Creer` et `LiensTracabiliteController.Creer` appelaient
  `MaturiteCalculatorService.RecalculerEtPersisterAsync` **avant** `SaveChangesAsync()` de
  l'entité ajoutée — or le calculateur relit systématiquement les tables par requête SQL directe,
  donc l'entité fraîchement ajoutée (encore seulement trackée en mémoire) n'était pas vue. Même
  variante du bug déjà rencontré et corrigé à l'étape 4
  (`UpdateStatutPhaseAction`). **Un 4e site était infecté sans avoir été détecté à l'étape 5** :
  `QuestionsRegistreController.Creer` — son test passait par coïncidence (le niveau était déjà au
  plafond attendu avant l'ajout de la question, donc le bug ne changeait rien d'observable). Les
  4 sites ont été corrigés (un `SaveChangesAsync()` systématique juste avant tout appel à
  `RecalculerEtPersisterAsync`) et le test de l'étape 5 a été durci pour qu'il ne puisse plus
  masquer une régression de ce type. Un audit de tous les appels à `RecalculerEtPersisterAsync`
  dans le code a confirmé qu'aucun autre site n'était concerné.
- **Bug SQLite : `ORDER BY` sur une colonne `decimal`** — `ProblemesController.GetTous` triait
  par `ScoreCalcule` (decimal) directement en SQL via `OrderByDescending`, ce que le provider
  SQLite ne sait pas traduire (`System.NotSupportedException`), provoquant un 500 sur **tout**
  appel à la liste des problèmes dès qu'au moins un existait. Non détecté par les tests xUnit car
  aucun test n'appelait `GetTous` après une création (`Creer_probleme_calcule_le_score_cote_serveur`
  ne vérifiait que la réponse du POST). Découvert lors du test manuel en navigateur : la liste
  restait vide après création d'un problème alors que l'entité existait bien en base. Corrigé en
  matérialisant la liste puis en triant côté client (`.ToListAsync()` puis `.OrderByDescending()`
  en mémoire) ; un test `GetTous_problemes_les_retourne_tries_par_score_decroissant` a été ajouté
  pour couvrir ce chemin à l'avenir.
- Les points des étapes 1-5 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 7 — Mode entretien (2026-09-18)

### Contenu réalisé
- Deux décisions de conception validées avec l'utilisateur avant de coder :
  1. Les questions guidées "une à la fois" (Prompt Maître section 3) sont codées en dur pour les
     Phases 01 et 02 uniquement — seules phases du Guide dont chaque question correspond 1-pour-1
     à une `InformationRegistre`. Les Phases 03/04 (étapes de processus, problèmes quantifiés) et
     05-18 (non détaillées dans le Guide fourni) renvoient vers les vues détaillées déjà
     construites aux étapes 5/6 plutôt que d'inventer un mapping question→entité non spécifié par
     le Prompt Maître.
  2. Après validation d'une réponse, avancement **automatique** à la question suivante (pas de
     clic "Suivant" séparé) — optimisé pour la vitesse de saisie en RDV live.
- `Shared/ModeEntretien/QuestionGuidee.cs` : modèle statique des questions du Bloc A (6 questions
  Phase 01, 5 questions Phase 02, fidèles aux libellés du Guide des 18 phases), avec source par
  défaut et indicateur multiligne.
- Client Blazor : page `ModeEntretien.razor` (route `/projets/{id}/entretien/{numeroPhase?}`) —
  interface épurée (pas de drawer, pas de tableau, une seule question visible à la fois — Prompt
  Maître section 3), barre de progression, navigation précédent/suivant, navigation rapide entre
  les 18 phases par chips. Composant `EntretienQuestionGuidee.razor` : formulaire à un champ
  (réponse + source), gros boutons tactiles "Validé"/"À confirmer" (≥44px, section 3), qui créent
  ou mettent à jour directement une `InformationRegistre` existante (pas de duplication en cas de
  retour arrière puis re-validation). Pour les phases hors Bloc A, message explicite + bouton de
  redirection vers la vue détaillée de la phase (étape 4). Bouton d'accès "Mode entretien" ajouté
  sur la page Phase détaillée.
- Aucun changement côté Api : réutilise entièrement `InformationRegistreController` et
  `RegistresApiClient` posés à l'étape 5. 77/77 tests xUnit (aucun nouveau test backend — pas de
  nouvelle logique serveur à cette étape) passent sans régression.
- Flux vérifié dans un vrai navigateur (desktop et tablette 768px) : réponse à une question →
  avancement automatique confirmé, retour en arrière → réponse précédente correctement préremplie
  (donc une revalidation modifie l'entrée existante plutôt que d'en créer une seconde), phase hors
  Bloc A → redirection claire vers la vue détaillée, rendu tablette conforme à l'esprit "interface
  épurée, pas de surcharge visuelle" du Prompt Maître.

### Décisions d'architecture prises
- Voir les 2 décisions validées avec l'utilisateur en tête de section.
- Le rapprochement réponse-existante↔question se fait par correspondance exacte du libellé de la
  question sur les `InformationRegistre` de la phase (pas de nouvel identifiant dédié) : suffisant
  pour un ensemble fixe et codé en dur de questions, évite d'ajouter un champ supplémentaire au
  modèle de données pour cette seule fonctionnalité.

### Problèmes connus / points ouverts
- Aucun nouveau bug détecté — étape purement UI, réutilisant une Api déjà testée et éprouvée aux
  étapes précédentes.
- Les questions guidées du Bloc A sont limitées aux Phases 01/02 ; si le Guide des 18 phases est
  complété pour les Blocs B-E dans une future révision du Prompt Maître, `QuestionsGuideesParPhase`
  devra être étendu en conséquence (voir le TODO implicite déjà noté pour les noms de phases
  provisoires depuis l'étape 1).
- Les points des étapes 1-6 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 8 — Traçabilité (2026-09-18)

### Contenu réalisé
- Deux décisions de conception validées avec l'utilisateur avant de coder :
  1. Cas 1 de contradiction (Demande Phase 02 vs Problème quantifié Phase 04) détecté par une
     règle **structurelle simple** (la Demande — `InformationRegistre` créée par la question
     guidée de l'étape 7 — est Validée mais aucun `Probleme` du projet n'a de `ScoreCalcule > 0`),
     pas par une comparaison sémantique du texte, hors de portée d'un outil de gestion classique.
  2. Cas 2 (informations contradictoires) détecté par normalisation simple du libellé (casse et
     espaces ignorés) + comparaison texte des valeurs après `Trim()`, sans détection de synonymes
     ni de similarité.
- `Api/Services/TracabiliteService` : formalise en service dédié la détection d'orphelins
  (Fonctionnalité sans lien, Problème non couvert) déjà calculée de façon dupliquée dans
  `MaturiteCalculatorService` et les controllers depuis l'étape 6 — lecture seule, ne modifie rien.
- `Api/Services/ContradictionDetectorService` : couvre les 3 cas du Prompt Maître 4.3 — (1)
  Demande validée sans Problème quantifié (génère une `QuestionRegistre` **Bloquante**), (2) deux
  `InformationRegistre` au même libellé normalisé avec des valeurs différentes (génère une
  question Normale par groupe en contradiction), (3) `EtapeProcessus.ExempleValide = false` alors
  que la Phase 03 est Terminee (une question par étape non validée). Génération **idempotente** :
  une question ouverte avec un texte identique n'est jamais dupliquée en cas d'appels répétés.
  Référence directement `QuestionsGuideesParPhase` (Shared, étape 7) pour identifier le libellé
  exact de la question "Demande", plutôt que de dupliquer ce texte en dur dans l'Api.
- `Api/Controllers/TracabiliteController` (`[Authorize]`) : `GET .../tracabilite/alertes` (lecture
  seule), `POST .../tracabilite/detecter-contradictions` (déclenche la détection + génère les
  relances + recalcule `NiveauMaturite` si une question Bloquante a été créée).
- Client Blazor : `TracabiliteApiClient`, composant `BandeauAlertesTracabilite.razor` — alertes
  visuelles affichées en continu en haut de la page Domaine analysé (étape 6), bouton "Détecter
  les contradictions" déclenchant l'analyse à la demande avec confirmation du nombre de
  contradictions trouvées.
- Tests xUnit : `TracabiliteServiceTests` (orphelins détectés/non détectés selon les liens),
  `ContradictionDetectorServiceTests` (13 tests couvrant les 3 cas — détecté/non détecté pour
  chacun, non-duplication sur appel répété), `TracabiliteControllerTests` (alertes via HTTP, effet
  réel d'une contradiction sur `NiveauMaturite` via l'API complète, non-duplication via HTTP).
  95/95 tests passent au total (77 hérités des étapes 1-7 + 18 nouveaux).
- Flux vérifié dans un vrai navigateur : création d'une Fonctionnalité orpheline → alerte affichée
  automatiquement sur la page Domaine ; création d'une Demande validée sans Problème → clic sur
  "Détecter les contradictions" → confirmation "1 contradiction détectée" → vérification que la
  `QuestionRegistre` de relance a bien été créée avec l'importance Bloquante attendue.

### Décisions d'architecture prises
- Voir les 2 décisions validées avec l'utilisateur en tête de section.
- Idempotence de la génération des questions de relance assurée par une simple vérification
  d'existence (même texte, statut Ouverte) avant insertion — pas de nouveau champ ajouté au modèle
  de données (ex. "généré automatiquement") pour cette seule fonctionnalité, jugé disproportionné.

### Problèmes connus / points ouverts
- Aucun nouveau bug détecté cette fois.
- Les points des étapes 1-7 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Thème visuel Solideo Digital (2026-09-18, hors plan des étapes numérotées)

### Contexte
Premier test d'usage réel par l'utilisateur après l'étape 8 (le développement avait jusque-là été
validé uniquement par moi via des serveurs de dev temporaires). Retour : l'ergonomie et la
disposition conviennent ; demande de reprendre l'identité visuelle du site vitrine Solideo Digital
plutôt que le thème violet par défaut de MudBlazor, l'outil étant destiné à être ouvert devant un
client en RDV (Prompt Maître, usage en direct pendant un rendez-vous). Timing tranché avec
l'utilisateur : appliqué maintenant plutôt qu'en fin de développement, pour que les étapes 9+
héritent directement du bon thème sans repasse ultérieure sur les écrans déjà construits.

### Contenu réalisé
- Charte reprise depuis `solideo-digital/public/css/variables.css` (site Laravel/Tailwind de
  l'utilisateur, dossier voisin sur disque) : or `#B8935E`, bleu marine `#1A2B3C`, beige `#F5F0E8`,
  police Inter.
- Vérification WCAG avant application (calcul de contraste) : l'or seul ne passe pas le seuil
  AA (2.85:1 sur blanc, texte blanc sur or 2.85:1 aussi) — jamais utilisable comme fond de texte
  blanc ni comme couleur de texte sur fond clair. Le bleu marine, lui, passe largement partout
  (14.4:1 sur blanc, 5.1:1 sur or). Palette du thème construite en conséquence : marine = couleur
  primaire (AppBar, boutons, texte), or = accent secondaire réservé aux surfaces où le texte pardessus reste en marine (`SecondaryContrastText` forcé en marine, pas le blanc par défaut du
  framework).
- `Client/Theme/SolideoDigitalTheme.cs` : `MudTheme` centralisé (palettes claire et sombre,
  typographie Inter), appliqué globalement via `MudThemeProvider` dans `MainLayout.razor`.
- Logo : détourage du monogramme "SD" depuis le logo source du site (fond beige plein cadre
  retiré pixel par pixel, le fichier `faviconSD.png` fourni s'étant avéré totalement transparent
  et inexploitable tel quel) pour un rendu propre sur l'AppBar marine ; réduit à 7 Ko. Favicon
  généré à partir du même monogramme, réduit de 1024×1024/1,4 Mo à 64×64/2,2 Ko.
- `index.html` : police Inter chargée depuis Google Fonts (hôte autorisé), favicon Solideo Digital.
- Couleur de l'indicateur de chargement Blazor harmonisée avec la charte (`#B8935E` au lieu du
  bleu par défaut du template).
- Rendu vérifié dans un vrai navigateur : page de connexion, tableau de bord, liste des projets,
  page Domaine analysé — AppBar marine avec monogramme doré, fond beige, boutons et liens marine,
  chips de statut lisibles, aucune régression de contraste observée.

### Décisions d'architecture prises
- Seul le monogramme "SD" (pas le logo complet avec le texte "SOLIDEO DIGITAL") est utilisé dans
  l'AppBar : le logo complet contenait une grande marge de fond beige plein cadre, disgracieux et
  lourd (238 Ko même redimensionné) une fois placé sur le fond marine de l'AppBar à petite taille.
- `SecondaryContrastText` explicitement fixé en marine dans le thème plutôt que laissé au blanc
  par défaut de MudBlazor, pour que tout composant utilisant `Color.Secondary` (boutons, chips)
  reste lisible sans avoir à y penser composant par composant.

### Problèmes connus / points ouverts
- Aucun bug — changement purement visuel, sans impact sur la logique métier ni les tests (95/95
  toujours au vert, ce changement ne touchant que le Client).
- Le logo complet (avec texte) n'est pas encore intégré nulle part (seul le monogramme est utilisé
  dans l'AppBar) — à réévaluer si un futur écran (page de connexion, export PDF) a besoin du logo
  complet ; il faudra alors le retravailler pour retirer sa marge morte plutôt que réutiliser le
  fichier source tel quel.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation par l'utilisateur avant commit.

## Étape 9 — Export Markdown (2026-09-18)

### Contenu réalisé
- Trois décisions de conception validées avec l'utilisateur avant de coder :
  1. Livraison par **téléchargement ZIP** (pas d'écriture sur le système de fichiers du serveur) :
     cohérent avec un déploiement conteneurisé et un usage nomade en RDV client, contrairement à
     un chemin `clients/[nom]/` littéral sur disque qui aurait mélangé configuration et exports
     et n'aurait rien produit d'immédiatement récupérable sur l'appareil utilisé en RDV.
  2. Contenu des 3 sections dynamiques de chaque fichier de phase filtré **strictement par
     PhaseId** : Informations validées = `InformationRegistre.Statut=Valide` de cette phase,
     Points ouverts = `InformationRegistre.Statut=AConfirmer` + `QuestionRegistre` Ouvertes de
     cette phase. `RisqueRegistre` et `DecisionRegistre` n'ont pas de `PhaseId` dans le modèle —
     listés uniquement dans `ANALYSE.md`, jamais répétés par phase.
  3. `ANALYSE.md` reprend tout ce que le Prompt Maître décrit explicitement à cet endroit : fiche
     client, niveau de maturité, encart Demande vs Problème pressenti (§02), tableau des
     problèmes priorisés avec score et couverture (§04), Risques, Décisions, état des 18 phases,
     alertes de traçabilité actives (étape 8).
- `Api/Services/MarkdownExportService` : génère l'archive ZIP en mémoire (`ZipArchive` sur
  `MemoryStream`, aucune écriture disque), un fichier par phase au format
  `{numero:D2}-{slug-du-nom-de-phase}.md` + `ANALYSE.md`, en respectant le template standardisé du
  Prompt Maître 4.4 (`## Objectif de la phase`, `## Informations validées (✅)`,
  `## Points ouverts (⚠️/⛔)`, `## Décisions prises`) pour chaque fichier de phase.
- `Api/Controllers/ExportsController` (`[Authorize]`) : `GET .../exports/markdown` retourne
  directement le flux ZIP (`Content-Type: application/zip`, `Content-Disposition` avec le nom de
  fichier `analyse-{slug-client}.zip`).
- Client Blazor : `ExportsApiClient` récupère les octets puis délègue le téléchargement réel à un
  petit script d'interop JS (`wwwroot/js/download.js`, `Blob` + lien `<a download>` synthétique)
  — Blazor WASM n'a pas de mécanisme natif pour déclencher un téléchargement de fichier binaire.
  Composant `BoutonExportMarkdown.razor` réutilisable, intégré sur la page Phase (à côté de "Mode
  entretien") et sur la page Domaine analysé.
- Tests xUnit : `MarkdownExportServiceTests` (10 tests — présence des 19 fichiers, noms de
  fichiers de phase corrects, séparation stricte par PhaseId des informations et questions,
  Risques/Décisions dans ANALYSE.md uniquement, problèmes non couverts et alertes de traçabilité
  bien reportés), `ExportsControllerTests` (3 tests — Content-Type, nombre de fichiers via HTTP
  réel, 404 sur projet inexistant). 108/108 tests passent au total (95 hérités des étapes 1-8 +
  13 nouveaux).
- Flux vérifié de bout en bout : téléchargement réel via `curl` authentifié (ZIP valide, 19
  fichiers, contenu conforme au template), et clic du bouton dans un vrai navigateur (requête
  réseau `200 OK` vers l'endpoint d'export, déclenchement du téléchargement côté client sans
  erreur).

### Décisions d'architecture prises
- Voir les 3 décisions validées avec l'utilisateur en tête de section.
- Encodage `UTF8` explicitement **sans BOM** pour l'écriture des fichiers dans l'archive : un BOM
  en tête de fichier Markdown ne casse rien à l'affichage mais n'a aucune utilité et a été retiré
  après l'avoir repéré lors du test manuel du contenu réel du ZIP téléchargé.

### Problèmes connus / points ouverts
- Les noms de fichiers de phase générés (`05-acteurs-provisoire.md`, etc.) incluent le suffixe
  "(provisoire)" pour les phases 05-18, car ce suffixe fait partie du `Phase.Nom` stocké en base
  depuis l'étape 1 — le nom littéral donné en exemple au Prompt Maître 4.4
  (`18-transfertprompt-maitre.md`, sans "provisoire") ne peut donc pas être atteint exactement
  tant que ces noms de phase n'auront pas été révisés (point déjà ouvert depuis l'étape 1). Les
  noms de fichiers générés restent cohérents et lisibles en attendant.
- Les points des étapes 1-8 (seuils `NiveauParPhases` des Blocs B/C/D non fixés, 2FA/CrowdSec hors
  périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape 10 — Export transfert prompt maître (2026-09-18)

### Contenu réalisé
- Trois décisions de mapping validées avec l'utilisateur avant de coder (le Prompt Maître décrit
  l'existence de ce transfert vers les sections 1/2/4/9/10 d'un futur prompt maître de
  développement, sans détailler la correspondance champ-par-champ) :
  1. **§1 Contexte** : Demande (InformationRegistre de la question guidée Phase 02, étape 7) +
     les 3 `Probleme` au `ScoreCalcule` le plus élevé (étape 6) — ancre l'objectif métier à la
     fois sur la demande du client et sur les problèmes réels quantifiés, dans l'esprit du Guide
     18 phases ("éviter solution avant besoin").
  2. **§9 Rôles & permissions** : chaque `Acteur` du domaine analysé devient une ligne "Rôle", ses
     `Permission` résumées en "Accès" — correspondance directe avec le modèle de l'étape 6.
  3. **§4 Domaine métier** : `Entite` (Code, Attributs) + `Fonctionnalite` groupées et triées par
     `PrioriteMoSCoW`. **§10 Modules** : suggestion générée à partir des mêmes `Fonctionnalite`
     triées par priorité MoSCoW, explicitement étiquetée "proposition, à ajuster" dans le texte
     généré — l'outil n'a pas connaissance des dépendances techniques réelles entre modules.
- `Api/Services/PromptMaitreTransfertService` : construit le bloc de texte Markdown complet en
  mémoire (pas de fichier), une méthode dédiée par section (1/2/4/9/10).
- `Api/Controllers/ExportsController` étendu : `GET .../exports/prompt-maitre` retourne le contenu
  en **JSON** (pas en téléchargement de fichier, contrairement à l'export Markdown de l'étape 9) —
  l'usage prévu par le Prompt Maître est un copier-coller direct dans une nouvelle conversation,
  pas un enregistrement sur disque.
- Client Blazor : `PromptMaitreDialog.razor` affiche le texte généré dans une zone de texte en
  lecture seule (police monospace, scrollable) avec un bouton "Copier" utilisant l'API navigateur
  `navigator.clipboard.writeText` via interop JS. Composant `BoutonExportPromptMaitre.razor`
  réutilisable, ajouté à côté du bouton d'export Markdown sur les pages Phase et Domaine analysé.
- Tests xUnit : `PromptMaitreTransfertServiceTests` (10 tests — présence des 5 sections, reprise
  de la Demande, limitation à 3 problèmes les mieux priorisés, reprise de la stack envisagée,
  entités et fonctionnalités listées et triées par priorité, résumé des permissions y compris le
  raccourci "Tout", mention explicite "proposition" en section 10), `PromptMaitreExportController
  Tests` (2 tests — réponse JSON, 404 sur projet inexistant). 120/120 tests passent au total
  (108 hérités des étapes 1-9 + 12 nouveaux).
- Flux vérifié dans un vrai navigateur avec des données réelles du seeder (projet "Gestion
  adhésions et dons") : ouverture du dialog, contenu affiché correspondant exactement à la réponse
  API vérifiée séparément via `curl`, clic sur "Copier" sans erreur (le contenu réel du
  presse-papier système n'est pas vérifiable depuis un test automatisé en environnement headless
  sandboxé — limite de l'outil de test, pas du code).

### Décisions d'architecture prises
- Voir les 3 décisions de mapping validées avec l'utilisateur en tête de section.
- Retour JSON plutôt que fichier téléchargé pour ce seul export (contrairement à l'export Markdown
  ZIP de l'étape 9) : cohérent avec l'usage "prêt à coller" explicitement décrit au Prompt Maître
  4.4, un fichier à ouvrir puis copier aurait ajouté une étape inutile.

### Problèmes connus / points ouverts
- Aucun bug détecté cette fois.
- Le contenu généré pour la section 2 (Stack technique) reste minimal (juste le nom de la stack
  envisagée, texte libre) car l'outil ne modélise pas le détail technique d'une stack cible — reste
  cohérent avec le Prompt Maître, qui ne prévoit pas non plus de saisie structurée pour cela.
- Les points des étapes 1-9 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.

## Étape Dashboard — Vue d'ensemble multi-projets (2026-09-18)

### Contenu réalisé
- Dernière étape du plan de développement (Prompt Maître, section 10) : une vue d'ensemble
  multi-projets, remplaçant le message statique affiché depuis l'étape 2 sur la page racine `/`.
  Périmètre validé avec l'utilisateur : cartes projets + jauge de maturité + registre global des
  questions bloquantes (pas de graphiques additionnels ni de filtres — rester au plus proche du
  Prompt Maître, qui ne détaille pas plus que ces trois éléments pour cette étape).
- `Api/Controllers/DashboardController` : `GET api/dashboard`, lecture seule, protégé
  `[Authorize]`. Agrège des données déjà exposées ailleurs (Projets, QuestionRegistre) — aucune
  logique métier nouvelle, aucun recalcul : une carte par projet (nom, client, niveau de maturité
  déjà persisté, statut, nombre de phases terminées, nombre de questions bloquantes ouvertes) triée
  par nombre de questions bloquantes décroissant puis par nom, plus la liste globale des questions
  bloquantes ouvertes tous projets confondus.
- `Shared/Dtos/Dashboard/DashboardDto.cs` : `DashboardDto`, `ProjetResumeDto`, `QuestionBloquanteDto`.
- Client Blazor (`Client/Pages/Index.razor`, page racine `/`) : `DashboardApiClient` (nouveau
  service, même pattern que les autres wrappers HttpClient typés) consommé pour afficher une grille
  de cartes projets (nom, client, badge d'alerte si questions bloquantes ouvertes, barre de
  progression colorée représentant la maturité 0-5, statut, phases terminées / 18) suivie d'un bloc
  "Questions bloquantes — tous projets" listant chaque question avec son code et le nom du projet
  concerné, cliquable vers la page Domaine analysé du projet correspondant.
- Tests xUnit : `DashboardControllerTests` (5 tests — présence de tous les projets avec leur
  maturité, comptage des phases terminées, comptage des questions bloquantes ouvertes par projet,
  registre global incluant les questions bloquantes ouvertes, exclusion des questions bloquantes
  résolues). 125/125 tests passent au total (120 hérités des étapes 1-10 + 5 nouveaux).
- Flux vérifié dans un vrai navigateur (Chrome headless + Chrome DevTools Protocol) : connexion
  réelle via le formulaire, création d'un client/projet/question bloquante de test via l'API,
  rechargement du tableau de bord confirmant l'affichage correct de la nouvelle carte projet (badge
  d'alerte, jauge de maturité, phases terminées) et de la question bloquante dans le registre
  global — aucune erreur console. Données de test supprimées après vérification (statut 204).

### Bug résolu — suite de tests instable après ajout du Dashboard
- En ajoutant `DashboardControllerTests`, l'exécution de la suite **complète** (`dotnet test`)
  s'est mise à échouer sur 9 tests de `DomaineControllerTests`, systématiquement lors de la
  création du host de test (`CreateAuthenticatedClientAsync` → `WebApplicationFactory`), alors que
  ces mêmes tests passaient à 100% une fois isolés (`--filter`). Diagnostic : le message d'erreur
  réel (tronqué dans le résumé xUnit par défaut) était `System.IO.IOException: The configured user
  limit (128) on the number of inotify instances has been reached`. Chaque
  `WebApplicationFactory` démarre un host en environnement "Development", qui active par défaut le
  rechargement à chaud de la configuration (`FileSystemWatcher` sur les `appsettings.*.json`,
  consommant une instance `inotify` par host). La suite complète crée des dizaines de factories
  l'une après l'autre ; la limite système Linux par défaut (128 instances par utilisateur) finit
  par être atteinte avant la fin de la suite. Ce n'était pas une régression liée au code du
  Dashboard : la suite était simplement passée, avec l'ajout de ces 5 tests, au-dessus du seuil qui
  déclenchait le symptôme.
  - Fix retenu : `Api.Tests/AnalyseProjetWebApplicationFactory.cs` désactive explicitement le
    rechargement de configuration à chaud pour les hosts de test
    (`builder.UseSetting("hostBuilder:reloadConfigOnChange", "false")`), ce qui supprime la
    création du `FileSystemWatcher` — donc la consommation d'instances `inotify` — sans toucher à
    `Program.cs` ni à la configuration d'environnement réelle. Solution portable (pas de dépendance
    à une limite système modifiée manuellement sur chaque machine ou en CI), contrairement à
    l'alternative écartée (augmenter `fs.inotify.max_user_instances`).
  - Un essai intermédiaire (désactiver la parallélisation des classes de test via
    `xunit.runner.json`) n'a pas résolu le problème (le vrai goulot était le nombre total de hosts
    créés, pas leur concurrence) — retiré après l'avoir confirmé inefficace, pour ne garder que le
    changement réellement nécessaire.
  - Suite complète confirmée stable après correction : 125/125, exécutée deux fois de suite.

### Décisions d'architecture prises
- Cartes projets + jauge de maturité + registre global des questions bloquantes : périmètre validé
  avec l'utilisateur avant de coder, sans étendre au-delà (pas de graphiques de tendance, pas de
  filtres avancés — hors périmètre du Prompt Maître pour cette étape).
- `DashboardController` reste strictement en lecture : aucun recalcul de maturité ni d'aucune autre
  donnée déclenché depuis cette vue — il ne fait que lire les valeurs déjà persistées par les
  services dédiés (`MaturiteCalculatorService` etc.), cohérent avec l'interdiction de logique
  métier dans les Controllers (Prompt Maître section 14).

### Problèmes connus / points ouverts
- Aucun bug fonctionnel détecté sur le Dashboard lui-même après résolution du problème
  d'environnement de test ci-dessus.
- Les points des étapes 1-10 (noms de phases 5-18 provisoires, seuils `NiveauParPhases` des Blocs
  B/C/D non fixés, 2FA/CrowdSec hors périmètre code V1) restent valables.
- **Le plan de développement du Prompt Maître (section 10, étapes 1 à 10 + Dashboard) est
  maintenant complet.**

### Fichiers créés/modifiés
Voir `git log` / `git status` — en attente de validation de l'étape par l'utilisateur avant commit.
