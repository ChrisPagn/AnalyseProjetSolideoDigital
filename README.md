# AnalyseProjetSolideoDigital

Outil interne Solideo Digital pour mener des analyses de besoin structurées en 18 phases avec
un client : traçabilité complète demande → problème → besoin → fonctionnalité, utilisable en
rendez-vous client, avec export final vers un prompt maître de développement (Laravel / Blazor /
Spring Boot selon le projet cible).

Le déroulé des 18 phases et les règles métier (calcul de maturité, détection de contradictions,
export) suivent le **Prompt Maître** et le **Guide des 18 phases** — voir `docs/`.

## Stack technique

- **Client** — Blazor WebAssembly, [MudBlazor](https://mudblazor.com/)
- **Api** — ASP.NET Core Web API (.NET 9), Entity Framework Core
- **Base de données** — SQLite en développement, MySQL en production (via Pomelo.EntityFrameworkCore.MySql)
- **Authentification** — ASP.NET Core Identity, mono-utilisateur, cookie HttpOnly
- **Hébergement** — Docker (image unique Api + fichiers statiques du Client) derrière Caddy (reverse proxy, HTTPS automatique)

Architecture 3 couches : `Client` (UI) / `Api` (Controllers = orchestration uniquement) /
`Shared` (DTOs, Enums) — aucune logique métier dans le Client ni dans les Controllers ; elle vit
dans les services de `Api/Services`.

## Prérequis (développement local)

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

## Démarrage en local

```bash
# Restaurer et builder la solution complète
dotnet build

# Lancer l'Api (port 5118)
dotnet run --project Api/Api.csproj --urls "http://localhost:5118"

# Dans un autre terminal, lancer le Client (port 5138)
dotnet run --project Client/Client.csproj --urls "http://localhost:5138"
```

Le compte admin est créé automatiquement au premier démarrage à partir de la configuration
`AdminAccount:Email` / `AdminAccount:Password` (user-secrets en dev, variables d'environnement
en production — voir [DEPLOIEMENT.md](DEPLOIEMENT.md)).

```bash
# Configurer les identifiants admin en développement (une seule fois)
dotnet user-secrets set "AdminAccount:Email" "vous@exemple.fr" --project Api/Api.csproj
dotnet user-secrets set "AdminAccount:Password" "MotDePasseRobuste123!" --project Api/Api.csproj
```

## Tests

```bash
dotnet test Api.Tests/Api.Tests.csproj
```

## Déploiement

Voir [DEPLOIEMENT.md](DEPLOIEMENT.md) pour héberger l'application via Docker + Caddy.

## Licence

Propriétaire — voir [LICENSE](LICENSE). Usage interne Solideo Digital uniquement.
