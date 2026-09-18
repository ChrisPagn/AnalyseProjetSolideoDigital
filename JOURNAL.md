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
