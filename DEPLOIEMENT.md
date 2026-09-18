# Déploiement sur home server

Guide pour héberger AnalyseProjetSolideoDigital sur le home server existant, qui a déjà :
Docker + Compose, un conteneur `caddy` unique servant tous les sites (réseau Docker externe
`web`), CrowdSec sur l'hôte (`127.0.0.1:8080`), et la convention `~/docker/<projet>/` avec
`docker-compose.yml`, `container_name` explicite et bind mounts.

Ce projet **ne fait pas tourner son propre Caddy** : un seul conteneur (`analyseprojet-app`)
rejoint le réseau `web` existant, sans aucun port publié sur l'hôte — le Caddy déjà en place
route vers lui par nom de conteneur.

## 1. Préparer le dossier sur le serveur

```bash
mkdir -p ~/docker/analyseprojet
cd ~/docker/analyseprojet
git clone https://github.com/ChrisPagn/AnalyseProjetSolideoDigital.git .
```

## 2. Configurer les secrets (`.env`)

```bash
cp .env.example .env
```

Éditez `.env` et renseignez `APP_DOMAIN`, `ADMINACCOUNT__EMAIL`, `ADMINACCOUNT__PASSWORD` (voir
les commentaires du fichier — le mot de passe doit rester entre apostrophes). `.env` n'est jamais
commité (exclu par `.gitignore`) — c'est le seul endroit où mettre de vraies valeurs sur le
serveur. `docker-compose.yml` refuse de démarrer si l'une de ces variables manque
(`${VAR:?message}`).

## 3. Préparer le dossier de données

```bash
mkdir -p ~/docker/analyseprojet/data
sudo chown -R 1654:1654 ~/docker/analyseprojet/data
```

`1654` est l'UID non-root sous lequel le conteneur tourne (`$APP_UID` de l'image
`mcr.microsoft.com/dotnet/aspnet:9.0`, voir `Dockerfile`) — sans ce `chown`, l'écriture de la
base SQLite et des clés Data Protection dans `./data` échoue au démarrage.

## 4. Construire et démarrer

```bash
docker compose up -d --build
docker compose ps
docker compose logs -f
```

Le service rejoint le réseau externe `web` (déjà créé par la stack Caddy). Aucun port n'est
publié sur l'hôte — c'est un choix volontaire, pas un oubli : la seule entrée vers l'application
est le Caddy déjà en place, et le port 8080 de l'hôte est réservé à CrowdSec.

## 5. Ajouter le site au Caddy existant

Le bloc à ajouter est documenté dans `Caddyfile.exemple` de ce dépôt. Ouvrez le Caddyfile réel
(celui monté par le conteneur `caddy` existant) et collez-y ce bloc.

```bash
# Depuis le dossier qui contient le Caddyfile principal du serveur :

# Valider la syntaxe avant de recharger (Caddy refuse de recharger un fichier invalide, mais
# autant s'en assurer explicitement avant de toucher un Caddy qui sert déjà d'autres sites)
docker exec caddy caddy validate --config /etc/caddy/Caddyfile

# Recharger sans coupure des autres sites déjà servis par ce Caddy
docker exec caddy caddy reload --config /etc/caddy/Caddyfile
```

DuckDNS pointe déjà vers l'IP du serveur pour ce sous-domaine — aucun nouvel enregistrement à
créer. Caddy obtient et renouvelle le certificat Let's Encrypt automatiquement au premier accès.

## 6. Mettre à jour l'application

```bash
cd ~/docker/analyseprojet
git pull
docker compose up -d --build
```

`./data` (base SQLite + clés Data Protection) est préservé entre les mises à jour : les sessions
existantes restent valides après un rebuild, et les données ne sont jamais perdues par un
déploiement.

## 7. Sauvegardes

**Ne jamais faire un `tar`/`cp` direct sur le fichier `.db` pendant que l'application tourne** —
SQLite peut être en écriture au même instant, ce qui produirait une sauvegarde corrompue ou
incohérente. Utiliser `sqlite3 .backup`, qui gère la cohérence même à chaud :

```bash
mkdir -p ~/docker/analyseprojet/backups
sqlite3 ~/docker/analyseprojet/data/analyseprojet.db \
  ".backup '${HOME}/docker/analyseprojet/backups/analyseprojet-$(date +%Y%m%d-%H%M).db'"
```

À planifier via cron (ex. une fois par jour). Pensez à purger les sauvegardes les plus anciennes
périodiquement pour ne pas remplir le disque.

## 8. Arrêter / redémarrer

```bash
docker compose down         # arrêt (les données de ./data sont conservées)
docker compose up -d        # redémarrage
docker compose restart analyseprojet  # redémarrer uniquement ce service
```

## 9. Vérifications post-déploiement

À faire une fois après le premier déploiement (et après toute modification touchant
l'authentification ou le reverse proxy) :

```bash
# 1. Un endpoint protégé doit répondre 401 sans cookie de session — api/auth/me est
#    volontairement public (utilisé par le Client pour savoir s'il faut rediriger vers
#    /connexion, il répond toujours 200 avec {"estAuthentifie":false,...} sans session) ;
#    prendre un endpoint réellement protégé comme api/projets.
curl -sD - -o /dev/null https://analyseprojetsolideodigital.solideodigital.duckdns.org/api/projets
#    → doit répondre 401, sans erreur TLS

# 2. Rate limiting : 6 tentatives de login rapprochées depuis la même IP doivent renvoyer 429
#    sur la 6e (politique "login" : 5 requêtes / minute / IP — voir Api/Program.cs).
for i in $(seq 1 6); do
  curl -s -o /dev/null -w "%{http_code}\n" -X POST \
    https://analyseprojetsolideodigital.solideodigital.duckdns.org/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"Email":"test@exemple.fr","Password":"mauvais"}'
done
#    → les 5 premières lignes: 400/401 (identifiants invalides), la 6e: 429

# 3. La session doit survivre à un redémarrage du conteneur (preuve que les clés Data
#    Protection sont bien persistées dans ./data/keys, pas régénérées à chaque démarrage) :
#    connectez-vous dans le navigateur, notez l'heure, puis :
docker compose restart analyseprojet
#    → rafraîchir la page : toujours connecté, pas de redirection vers /connexion
```

## Dépannage rapide

- **L'application ne démarre pas** : `docker compose logs analyseprojet` — cause la plus
  fréquente : `.env` incomplet (le compose refuse de démarrer et l'indique explicitement grâce à
  `${VAR:?message}`), ou permissions sur `./data` (voir étape 3).
- **429 sur toutes les requêtes / cookie jamais Secure** : signe que `X-Forwarded-For` /
  `X-Forwarded-Proto` n'arrivent pas jusqu'à l'app, ou que `KnownNetworks`/`KnownProxies` n'ont
  pas été vidés côté `Api/Program.cs` — vérifiez que le bloc Caddy pointe bien vers
  `analyseprojet-app:8080` et que les deux conteneurs sont sur le réseau `web`.
- **Connexion admin refusée** : le compte admin n'est créé qu'au tout premier démarrage à partir
  des valeurs de `.env` ; les changer après coup ne met pas à jour le compte existant — il faut
  changer le mot de passe depuis l'application elle-même.
- **Site injoignable alors que le conteneur tourne** : vérifiez que le bloc a bien été ajouté au
  Caddyfile principal (pas seulement à `Caddyfile.exemple`, qui n'est jamais monté) et que
  `caddy reload` n'a pas renvoyé d'erreur.

## Test local (sans toucher au serveur)

`docker-compose.local.yml` permet de builder et tester l'image sur votre poste de développement,
en dehors de tout Caddy :

```bash
docker network create web  # si le réseau n'existe pas déjà en local
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build
curl -s -o /dev/null -w "%{http_code}\n" http://127.0.0.1:8080/
curl -s -o /dev/null -w "%{http_code}\n" http://127.0.0.1:8080/api/auth/me
```

Le port n'est publié que sur `127.0.0.1` — ce fichier ne doit jamais être utilisé sur le home
server (le port 8080 de l'hôte y est réservé à CrowdSec).
