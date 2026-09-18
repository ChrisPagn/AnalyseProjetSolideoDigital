# Déploiement sur home server

Guide pour héberger AnalyseProjetSolideoDigital sur un serveur personnel, accessible depuis
Internet via un nom de domaine avec HTTPS automatique.

Architecture : un conteneur `app` (Api + fichiers statiques du Client compilés dedans, un seul
port 8080 interne) derrière un conteneur `caddy` qui fait office de reverse proxy et gère les
certificats TLS (Let's Encrypt) automatiquement.

## 1. Prérequis sur le serveur

- Docker Engine + le plugin Docker Compose (`docker compose`, pas l'ancien `docker-compose`
  autonome — les commandes ci-dessous utilisent la syntaxe moderne)
- Les ports **80** et **443** libres et redirigés vers le serveur depuis votre box/routeur
  (port forwarding) si le serveur est derrière un NAT domestique
- Un nom de domaine qui pointe vers l'IP publique de votre connexion

```bash
# Installer Docker (Debian/Ubuntu) si ce n'est pas déjà fait
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# Se déconnecter/reconnecter (ou `newgrp docker`) pour que l'appartenance au groupe prenne effet
```

## 2. Nom de domaine (DuckDNS)

Le `Caddyfile` du dépôt pointe déjà vers un sous-domaine DuckDNS :
`analyseprojetsolideodigital.solideodigital.duckdns.org`. Si vous utilisez ce domaine :

1. Créez un compte sur [duckdns.org](https://www.duckdns.org) et ajoutez le sous-domaine
   `analyseprojetsolideodigital` (ou adaptez le `Caddyfile` si vous préférez un autre nom).
2. Faites pointer ce sous-domaine vers l'IP publique de votre connexion domestique.
3. Comme cette IP change probablement (IP dynamique), installez un petit script/cron qui met à
   jour l'enregistrement DuckDNS périodiquement (DuckDNS fournit un script officiel très simple,
   `duck.sh`, à lancer via cron toutes les 5 minutes).

Si vous utilisez un autre nom de domaine, remplacez la première ligne du `Caddyfile` par le
vôtre — Caddy s'occupe ensuite automatiquement d'obtenir et de renouveler le certificat HTTPS
tant que le domaine pointe bien vers le serveur et que les ports 80/443 sont accessibles depuis
Internet (Let's Encrypt en a besoin pour valider le domaine).

## 3. Récupérer le projet sur le serveur

```bash
git clone https://github.com/ChrisPagn/AnalyseProjetSolideoDigital.git
cd AnalyseProjetSolideoDigital
```

## 4. Configurer les secrets (`.env`)

```bash
cp .env.example .env
```

Éditez `.env` et renseignez :

- `ADMINACCOUNT__EMAIL` — votre e-mail (compte admin unique de l'application)
- `ADMINACCOUNT__PASSWORD` — un mot de passe robuste (min. 12 caractères, au moins 1 majuscule,
  1 chiffre, 1 caractère spécial)

`.env` n'est jamais commité (exclu par `.gitignore`) — c'est le seul endroit où mettre de vraies
valeurs sur le serveur.

Par défaut, `docker-compose.yml` utilise **SQLite** avec un volume local (`./data`), ce qui
suffit largement pour un usage interne mono-instance. Pour passer en MySQL, voir la section 7.

## 5. Démarrer l'application

```bash
docker compose up -d --build
```

Cela construit l'image (build .NET en multi-stage, voir `Dockerfile`), puis démarre les deux
conteneurs (`app` et `caddy`). Le premier démarrage peut prendre quelques minutes (compilation).

```bash
# Vérifier que tout tourne
docker compose ps

# Suivre les logs (utile pour voir Caddy obtenir le certificat HTTPS la première fois)
docker compose logs -f
```

Une fois les logs Caddy stabilisés (plus d'activité ACME/Let's Encrypt), l'application est
accessible à `https://votre-domaine`.

## 6. Mettre à jour l'application (nouvelle version)

```bash
cd AnalyseProjetSolideoDigital
git pull
docker compose up -d --build
```

Le volume `./data` (base SQLite) est préservé entre les mises à jour — seul le code applicatif
est reconstruit.

## 7. Passer à MySQL (optionnel)

Si vous préférez MySQL plutôt que SQLite (recommandé si plusieurs personnes doivent accéder à
l'outil en même temps, SQLite gérant mal les écritures concurrentes) :

1. Ajoutez un service MySQL à `docker-compose.yml` (ou pointez vers une instance MySQL déjà
   existante sur votre serveur).
2. Dans `.env`, remplacez la configuration SQLite par :
   ```
   DATABASE_PROVIDER=MySql
   CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=mysql;Database=analyseprojet;User=analyseprojet;Password=CHANGER_MOI;
   ```
3. Adaptez `docker-compose.yml` pour transmettre ces deux variables au service `app` (au lieu
   des lignes `ConnectionStrings__DefaultConnection` / `DatabaseProvider` codées en dur pour
   SQLite) et ajoutez `depends_on: mysql` sur le service `app`.

## 8. Sauvegardes

Avec SQLite (configuration par défaut), toute la base de données est le fichier
`./data/analyseprojet.db` sur le serveur. Sauvegardez ce dossier régulièrement :

```bash
# Exemple simple : copie datée, à planifier via cron
tar czf "analyseprojet-backup-$(date +%Y%m%d).tar.gz" data/
```

## 9. Arrêter / redémarrer

```bash
docker compose down        # arrêt (les données du volume ./data sont conservées)
docker compose up -d        # redémarrage
docker compose restart app  # redémarrer uniquement l'application, sans toucher à Caddy
```

## Dépannage rapide

- **Le certificat HTTPS ne s'obtient pas** : vérifiez que le domaine pointe bien vers l'IP
  publique actuelle du serveur (`dig +short votre-domaine`) et que les ports 80/443 sont bien
  redirigés depuis votre box vers le serveur. Consultez `docker compose logs caddy`.
- **L'application ne démarre pas** : `docker compose logs app` — les causes les plus courantes
  sont un `.env` incomplet (mot de passe admin ne respectant pas la politique de complexité) ou
  un conflit de port si autre chose écoute déjà sur 80/443 sur le serveur.
- **Connexion admin refusée** : le compte admin n'est créé qu'au tout premier démarrage à partir
  des valeurs de `.env` ; si vous les changez après coup, il faut modifier le mot de passe depuis
  l'application elle-même ou réinitialiser la base.
