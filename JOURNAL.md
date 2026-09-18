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
