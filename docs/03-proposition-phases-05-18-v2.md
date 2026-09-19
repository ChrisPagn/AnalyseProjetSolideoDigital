# Proposition — Mode entretien guidé, Phases 05 à 18 (v2, corrigée après relecture)

Version corrigée suite à une relecture externe (voir historique de conversation) qui a
identifié 6 anomalies et plusieurs points mineurs dans la v1 (`docs/02 Guide18phasesB.pdf`).
Ce document remplace la v1 comme référence pour l'implémentation à venir.

**Constat qui cadre cette proposition** (inchangé depuis la v1) : le Guide des 18 phases ne
détaille que les Phases 01-04 (Bloc A). Le Prompt Maître d'analyse (celui qui nous guide pour
coder AnalyseProjetSolideoDigital — à ne pas confondre avec le **Prompt Maître de
développement**, celui que l'outil génère en Phase 18 pour un futur projet client) ne fait que
lister les entités de données pour 05-18, sans script de questions. Les noms de phases 05-18
sont eux-mêmes marqués "(provisoire)" dans le code (`CreateProjetAction.cs`).

## Phases 03/04 — traitées en priorité, avant le Lot D (mise à jour 2026-09-19)

Signalé par l'utilisateur en test réel : les Phases 03 (Processus métier) et 04 (Problèmes et
besoins) n'ont, elles non plus, jamais eu de mode entretien guidé codé — seules 01 et 02 en ont
un. Laisser ce trou au milieu du parcours (01/02 guidées, 03/04 vides, 05-07 guidées) n'a pas de
sens pour un usage réel en RDV. Décision actée : insérer un lot dédié **Lot C-bis (Phases
03/04)** immédiatement après le Lot C (Documents) et avant le Lot D (Fonctionnalités/
Automatisations) — pendant qu'on est dans le rythme du Mode Entretien plutôt que d'attendre la
fin des 18 phases.

Contrairement aux Phases 05-18 traitées par le reste de ce document, le contenu des questions de
03/04 existe déjà intégralement dans le Guide des 18 phases source (pages 3-5 : questions
principales, relance, bloquantes, tout est rédigé) — pas de travail de rédaction à refaire,
seulement du code. De plus, `Processus`/`EtapeProcessus` et `Probleme` portent déjà nativement
leur propre mécanisme de confiance (`ExempleValide`/`ExempleDescription`, `Gravite`/
`ScoreCalcule`) — contrairement à Acteur/Entite/DocumentMetier, elles n'ont pas besoin du
mécanisme Source/Statut compagnon (Option B) posé au Lot B : ce lot est donc plus proche du Lot C
en complexité qu'un nouveau Lot B.

**Phase 03 — Processus métier** : `Processus` (Nom, Declencheur) + `EtapeProcessus` (Ordre,
Acteur, Action, Outil, DureeEstimee, ErreursConnues, ExempleValide, ExempleDescription) —
entités déjà en place (étape 6). Questions du Guide 18 phases (page 3) : processus à améliorer en
priorité, déclencheur et fin du processus, étapes dans l'ordre, puis pour chaque étape qui la
réalise/avec quel outil/quelles infos/quel résultat/combien de temps/quelles erreurs — avec la
"règle de la cascade" (ne jamais sauter à la solution technique, creuser dès qu'un outil est
cité). Question bloquante : déclencheur et fin de processus doivent être clairs ; toute étape
répondue "ça dépend de la personne" doit être marquée et creusée. Une étape ne passe Validée que
si `ExempleValide = true` (un exemple concret vérifié), jamais sur la seule base d'une
description théorique.

**Phase 04 — Problèmes et besoins** : `Probleme` (Description, Gravite, Frequence,
ImpactTempsHeuresMois, CoutEstime, ScoreCalcule calculé côté serveur) — entité déjà en place.
Questions du Guide 18 phases (page 4-5) : les 3 problèmes principaux, celui qui fait perdre le
plus de temps/coûte le plus cher/génère le plus d'erreurs, comment il est traité actuellement,
fréquence exacte et temps perdu estimé (pour le score). Garde-fou anti-solution-prématurée
systématique : "existe-t-il une autre manière d'obtenir le même résultat que celle que vous
imaginez ?". Question bloquante : au moins un problème critique (🔴) clairement quantifié est
requis (nécessaire pour prioriser le MVP en Phase 14) ; toute contradiction Demande (Phase 02)
↔ Problème réel doit être résolue ou actée — déjà détectée par `ContradictionDetectorService`
(Cas 1), pas de nouveau code nécessaire sur ce point.

**Impact technique** : deux nouveaux composants Blazor (`EntretienProcessus.razor`,
`EntretienProblemes.razor`), aucune migration EF Core (aucun changement de schéma), aucune
extension du mécanisme Source/Statut. Noms de phases 3/4 déjà définitifs ("Processus métier",
"Problèmes et besoins" — jamais marqués "(provisoire)", contrairement à 05-18) : rien à changer
dans `CreateProjetAction.cs`/`DbSeeder.cs` pour ce lot.

## Changements v1 → v2 (résumé)

1. **Fonctionnalité orpheline** (Phase 08) : définition corrigée, `LienTracabilite` élargi.
2. **Ordre Synthèse/Validation inversé** : Phase 16 devient "Synthèse" (produit le brouillon),
   Phase 17 devient "Validation" (le décideur valide ce brouillon) — la numérotation Prompt
   Maître (1-18) est préservée, seul le contenu de 16 et 17 est interverti.
3. **Source/Statut sur les entités 05-09** : Option B retenue — `InformationRegistre`
   compagnon, lien polymorphe (`EntiteType` + `EntiteReferenceId`), pas de duplication de
   colonnes sur 5 tables.
4. **Statut "Validé" en Phase 04** : durci, exige un retour vers la Phase 03 + gravité
   manuelle possible en complément du score.
5. **Ordre 05/06** : acteurs créés en brouillon dès 01/03, permissions saisies en grille en
   fin de Phase 06 (pas "une par une" en Phase 05).
6. **MoSCoW** : "Non arbitré" par défaut en Phase 08, tranché en Phase 14 (plus de double
   saisie).
7. Given/When/Then rédigés par l'analyste, validés par le client (Phase 08).
8. Questions bloquantes 01-04 : explicitement liées à la création d'une `QuestionRegistre`.
9. Vocabulaire : "Prompt Maître d'analyse" vs "Prompt Maître de développement" désormais
   distingués partout.
10. Nouvelle section : **Notes de préparation** (hors numérotation 1-18).
11. Exemples concrets ajoutés à chaque phase ; distinction Fonctionnalité/Automatisation
    clarifiée.

---

## Notes de préparation (hors numérotation 1-18)

**Ce n'est pas une Phase** — pas de `Phase.Numero`, aucun registre, aucun impact sur la
maturité. Rattachée directement au `Projet`, disponible avant même que la Phase 01 ne
commence, pour préparer le RDV.

**Contenu** :
- Un champ `NotesPreparation` (texte libre, multiligne) sur l'entité `Projet` : secteur,
  concurrents connus, outils actuels déjà repérés, tout ce que l'analyste sait avant le RDV.
  Rempli manuellement — pas de recherche automatisée externe (LinkedIn, etc. — écarté :
  dépendance externe fragile, risque de conformité, gain marginal face au coût).
- Une checklist en lecture seule des questions principales des Phases 01-04 (puis 05-18 une
  fois codées), générée à partir de `QuestionsGuideesParPhase` — cochable pour se rassurer
  avant de partir en RDV, sans effet sur les registres (pense-bête, pas une donnée d'analyse).

**Impact technique** : `Projet.NotesPreparation` (string?, migration simple) + une page Client
"Préparation" accessible depuis la fiche Projet.

---

## Phase 05 — Acteurs

**Nom définitif proposé** : "Acteurs"

**Objectif** : Identifier qui interagit avec le futur outil.

**Entité cible** : `Acteur` (Nom, Fonction) — création de l'acteur uniquement ; ses permissions
sont saisies en Phase 06 (voir "Changement d'ordre" ci-dessous), pas ici.

**Changement d'ordre (correction v2)** : les permissions d'un acteur portent sur des Entités
(Phase 06) qui n'existent pas encore au moment de la Phase 05. La Phase 05 se limite donc à
identifier les acteurs eux-mêmes ; la matrice acteurs × entités (permissions) est saisie en une
fois à la fin de la Phase 06, sous forme de grille cochable (`MudDataGrid`, lignes = Acteurs,
colonnes = Entités, cases = Voir/Créer/Modifier/Supprimer/Valider) plutôt qu'un remplissage
acteur par acteur. Cohérent avec un usage RDV rapide (cocher va plus vite que décrire).

**Acteurs en brouillon dès 01/03 — reporté à l'implémentation (Lot B)** : les réponses de la
Phase 01 (décideur/valideur/utilisateur) et les `EtapeProcessus.Acteur` de la Phase 03 sont du
texte libre — en extraire automatiquement un `Acteur` structuré demanderait un parsing fragile
(ex. "Marie Dupont, DG" → quel Nom, quelle Fonction ?), risque de créer des Acteurs mal formés.
Décision actée avec l'utilisateur : pas de création automatique. À la place, la Phase 05 affiche
un **rappel contextuel en lecture seule** (les réponses textuelles pertinentes de 01/03) pour que
l'analyste les recopie en un clic conscient, sans extraction automatique — implémenté dans
`EntretienActeurs.razor`.

**Exemple concret** : pour un cabinet comptable, "Comptable" (fonction : gère les dossiers
clients), "Client" (fonction : dépose ses justificatifs), "Système de facturation existant"
(un acteur peut être un système, pas seulement une personne).

**Questions principales** (répétées pour chaque acteur restant à identifier au-delà des
brouillons issus de 01/03) :
- Nom ou rôle de la personne/du groupe/du système
- Sa fonction dans l'organisation

**Questions de relance** :
- Les brouillons issus de 01/03 sont présentés en premier pour confirmation/complément.
- "Cette personne agit-elle seule, ou certaines actions nécessitent-elles une validation par
  quelqu'un d'autre ?" — prépare la case "Peut valider" de la grille en Phase 06.

**Questions bloquantes** :
- Au moins un acteur doit exister en fin de phase.

**Liens avec les autres phases** :
- Depuis 01 (décideur/utilisateur final) et 03 (chaque étape attribuée à un acteur).
- Vers 06 (grille de permissions) et 08 (chaque Fonctionnalite peut être rattachée à un
  ActeurId).

**Critères de fin de phase** : tous les acteurs mentionnés en 01 et 03 sont repris ici.

---

## Phase 06 — Données (Entités du domaine)

**Nom définitif proposé** : "Données"

**Objectif** : Lister les objets métier que l'outil futur devra gérer, puis qualifier qui peut
faire quoi sur chacun.

**Entité cible** : `Entite` (Nom, Description, Attributs texte libre, Relations texte libre) +,
en fin de phase, la grille `Permission` (ActeurId × EntiteConcernee).

**Exemple concret** : pour une association, "Adhésion" (contient : nom du membre, date, montant
cotisation ; liée à "Membre" et à "Paiement").

**Questions principales** (répétées pour chaque entité identifiée) :
- Nom de cet objet métier
- À quoi sert-il / que représente-t-il en une phrase ?
- Quelles informations contient-il (liste libre → `Attributs`) ?
- Est-il lié à d'autres objets métier déjà cités (→ `Relations`) ?

**Puis, une fois toutes les entités listées** : grille de permissions — pour chaque paire
Acteur × Entite, cocher Voir/Créer/Modifier/Supprimer/Valider.

**Questions de relance** :
- Reprendre systématiquement les objets déjà cités en Phase 03 ("les informations manipulées à
  chaque étape esquissent déjà les entités du domaine").
- "Cet objet existe-t-il déjà quelque part (Excel, logiciel actuel) ou est-il nouveau ?" — relie
  à un DocumentMetier (Phase 07) si pertinent.

**Questions bloquantes** :
- Au moins une entité doit exister en fin de phase.
- Chaque acteur créé en Phase 05 doit avoir au moins une permission dans la grille avant la fin
  de cette phase (sinon acteur "coquille vide" — reprend la question bloquante de la v1, mais
  vérifiée ici, au bon moment, plutôt qu'en Phase 05 où les entités n'existaient pas encore).

**Liens avec les autres phases** :
- Depuis 03 (informations manipulées par le processus) et 05 (acteurs à qualifier).
- Vers 08 (`LienTracabilite` peut relier un Probleme à une Entite).

**Critères de fin de phase** : les entités mentionnées en Phase 03 sont reprises ici · chaque
entité a au moins une description ou un attribut · chaque acteur a au moins une permission.

---

## Phase 07 — Documents

**Nom définitif proposé** : "Documents"

**Objectif** : Recenser les documents que l'outil devra produire, recevoir ou archiver.

**Entité cible** : `DocumentMetier` (Type, Origine, Destination, Format, DureeConservation).

**Exemple concret** : "Devis" (Origine : commercial, Destination : client, Format : PDF,
Conservation : 10 ans — obligation comptable).

**Questions principales** (répétées pour chaque document identifié) :
- Quel est ce document (type) ?
- D'où vient-il / qui le produit (Origine) ?
- À qui est-il destiné (Destination) ?
- Sous quel format ?
- Doit-il être conservé, combien de temps ?

**Questions de relance** :
- Reprendre les outils/fichiers cités en Phase 03.
- "Ce document a-t-il une contrainte réglementaire de conservation (comptabilité, RGPD) ?"

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 03. Vers 10 (un document existant en Excel/papier devient une contrainte de
  migration).

**Critères de fin de phase** : les documents cités en Phase 03 sont repris ou explicitement
écartés.

---

## Phase 08 — Fonctionnalités

**Nom définitif proposé** : "Fonctionnalités"

**Objectif** : Traduire les problèmes validés (Phase 04) en fonctionnalités concrètes, avec
leurs critères d'acceptation.

**Entité cible** : `Fonctionnalite` (Nom, Description, Priorite MoSCoW = **NonArbitree par
défaut** — voir correction ci-dessous, ActeurId, Statut) + `CritereAcceptation` (Given/When/
Then, **rédigés par l'analyste et soumis au client pour validation**, pas dictés mot à mot par
le client) + `LienTracabilite`.

**Correction — définition de "Fonctionnalité orpheline"** (anomalie v1 corrigée) : la v1
disait qu'un `CritereAcceptation` suffisait à éviter l'orphelinage, ce qui rendait la règle
inopérante (toute Fonctionnalite peut recevoir un critère). Règle corrigée : une Fonctionnalite
est orpheline si elle n'a **aucun** `LienTracabilite` vers un `Probleme`, une `Entite`, **une
Contrainte, une Règle métier ou une Exigence non-fonctionnelle** (voir extension de
`LienTracabilite` ci-dessous) — le `CritereAcceptation` documente une Fonctionnalite, il ne la
justifie pas.

**Extension nécessaire de `LienTracabilite`** (impact modèle de données) : ajouter la
possibilité de lier une Fonctionnalite à une `InformationRegistre` de type Contrainte/Règle/
Exigence NF (Phases 10/11/13), pas seulement à un Probleme ou une Entite — pour couvrir les
fonctionnalités transversales (authentification, sauvegarde, journal d'audit) qui ne répondent
à aucun problème client direct mais découlent d'une contrainte ou d'une exigence. Concrètement :
un champ optionnel `InformationRegistreId` sur `LienTracabilite`, en plus de `ProblemeId`/
`EntiteId`/`CritereAcceptationId` déjà existants — la contrainte CHECK "au moins un lien requis"
s'étend pour inclure cette nouvelle colonne.

**Correction — MoSCoW non-arbitré** (anomalie v1 corrigée) : ajouter `NonArbitree` à l'enum
`PrioriteMoSCoW` (valeur par défaut à la création). La Priorite n'est définitivement tranchée
qu'en Phase 14 — évite la double saisie et le risque qu'une valeur choisie à la volée en Phase
08 ne soit jamais revue.

**Exemple concret** : Probleme PROB-002 "Les devis mettent 3 jours à être validés par manque de
visibilité" → Fonctionnalite F-005 "Tableau de bord des devis en attente de validation",
rattachée à l'Acteur "Comptable", critère : *Given un devis en attente depuis plus de 24h, When
le comptable ouvre le tableau de bord, Then le devis apparaît en tête de liste surligné*.

**Questions principales** (répétées pour chaque fonctionnalité identifiée) :
- Quelle fonctionnalité proposez-vous pour répondre à [reformulation du Probleme lié] ?
- Qui l'utilisera (Acteur de la Phase 05) ?
- (La Priorité MoSCoW n'est pas demandée ici — reste `NonArbitree`, tranchée en Phase 14.)

**Questions de relance** :
- Systématique : "Quel problème (Phase 04) cette fonctionnalité couvre-t-elle ?" → crée le
  `LienTracabilite`.
- Si aucun problème ne la justifie → vérifier si elle découle d'une Contrainte/Règle/Exigence
  NF déjà connue (même si ces phases n'ont pas encore été remplies, le lien peut être créé plus
  tard) ; sinon interroger sa légitimité.

**Questions bloquantes** :
- Chaque Probleme validé en Phase 04 doit être couvert par au moins une Fonctionnalite.
- Toute Fonctionnalite créée doit avoir un `LienTracabilite` (Probleme, Entite, ou
  Contrainte/Règle/Exigence NF).

**Liens avec les autres phases** :
- Depuis 04, 03 (étapes répétitives candidates).
- Depuis/vers 10, 11, 13 (fonctionnalités transversales justifiées par une contrainte/règle/
  exigence NF plutôt que par un problème).
- Vers 09 (candidate à l'Automatisation plutôt qu'à une fonctionnalité manuelle — voir critère
  de distinction ci-dessous).
- Vers 14 (arbitrage MoSCoW).

**Critères de fin de phase** : aucun Probleme sans Fonctionnalite qui le couvre · aucune
Fonctionnalite orpheline (au sens corrigé ci-dessus) · chaque Fonctionnalite a au moins un
CritereAcceptation.

---

## Phase 09 — Automatisations

**Nom définitif proposé** : "Automatisations"

**Objectif** : Identifier les actions qui peuvent se déclencher sans intervention humaine (ou
avec validation humaine minimale).

**Entité cible** : `Automatisation` (Declencheur, Condition, Action, ValidationHumaine).

**Clarification Fonctionnalité vs Automatisation** (point mineur v1 intégré) : une
Fonctionnalite est **pilotée** par un Acteur humain (il ouvre un écran, clique, décide) ; une
Automatisation se **déclenche seule** sur un événement, avec ou sans validation humaine avant
l'exécution finale de l'action. Critère de bascule simple à poser au client : *"est-ce que
quelqu'un doit décider de faire cette action à chaque fois, ou peut-elle partir toute seule dès
que la condition est remplie ?"* → première réponse = Fonctionnalite, seconde = Automatisation
(éventuellement avec ValidationHumaine = true si un simple "OK, envoie" suffit).

**Exemple concret** : Declencheur "date d'échéance de cotisation atteinte", Condition
"cotisation non payée", Action "envoi d'un email de relance", ValidationHumaine = false (part
seule) — vs. la Fonctionnalite F-005 ci-dessus qui reste pilotée par le comptable.

**Questions principales** (répétées pour chaque automatisation identifiée) :
- Quel événement déclenche cette automatisation ?
- Y a-t-il une condition à vérifier avant d'agir ?
- Que doit-il se passer automatiquement (l'action) ?
- Une personne doit-elle valider avant l'exécution réelle, ou peut-elle s'exécuter sans
  supervision ?

**Questions de relance** :
- Reprendre les étapes répétitives de 03 et les Fonctionnalites de 08 : pour chacune, appliquer
  le critère de bascule ci-dessus.
- "Que se passe-t-il si cette automatisation échoue ou envoie une information erronée ?" — matière
  à un RisqueRegistre si pertinent.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 03 et 08. Vers 11 (une Condition récurrente peut révéler une règle métier à
  documenter).

**Critères de fin de phase** : chaque automatisation a un déclencheur et une action clairement
formulés · le besoin de validation humaine est explicitement tranché.

---

## Phase 10 — Contraintes

**Nom définitif proposé** : "Contraintes"

**Objectif** : Recenser ce qui s'impose au projet de l'extérieur.

**Entité cible** : `InformationRegistre`, avec création de `QuestionRegistre` explicite pour
tout point resté ouvert (voir correction générale ci-dessous).

**Exemple concret** : "Budget : 8 000€ maximum, fixé par le trésorier — non négociable" (Source
Déclaratif, Statut Validé) ; "Le fichier Excel de suivi des adhésions doit être repris à
l'identique au démarrage" (contrainte de migration issue de la Phase 07).

**Questions principales** :
- Y a-t-il un budget déjà évoqué ou fixé ? (reprendre/trancher la réponse "à confirmer" de la
  Phase 01)
- Y a-t-il un délai ou une échéance à respecter ?
- Les documents/outils identifiés en Phase 07 doivent-ils être repris ou migrés ? Sous quelle
  forme ?
- Contrainte technique ou réglementaire déjà connue ?

**Questions de relance** :
- Chaque document Phase 07 marqué "à conserver"/"existant" est proposé automatiquement comme
  candidat à une contrainte de migration.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 01, 07. Vers 08 (justification de fonctionnalités transversales), 13 (contrainte
  technique → exigence non-fonctionnelle formalisée).

**Critères de fin de phase** : budget et délai explicitement tranchés · documents/outils de la
Phase 07 marqués "à conserver" sont repris.

---

## Phase 11 — Règles métier

**Nom définitif proposé** : "Règles métier"

**Objectif** : Documenter explicitement les règles de gestion.

**Entité cible** : `InformationRegistre`.

**Exemple concret** : "Une remise de 5% s'applique automatiquement au-delà de 500€ de
commande — règle appliquée depuis 2020, jamais écrite nulle part avant cet entretien."

**Questions principales** :
- Règles de calcul ou seuils à respecter ?
- Cas particuliers ou exceptions aux règles générales décrites en Phase 03 ?
- Cas limites (montant à zéro, actions concurrentes) ?

**Questions de relance** :
- Reprendre systématiquement toute réponse "ça dépend de la personne / pas de règle fixe" de la
  Phase 03 : à clarifier définitivement ici, ou acter explicitement comme non formalisée.
- Toute Condition de la Phase 09 qui semble récurrente/complexe est proposée pour formalisation.

**Questions bloquantes** :
- Toute règle non formalisée relevée en Phase 03 doit être reprise et statuée ici.

**Liens avec les autres phases** :
- Depuis 03, 09. Vers 08 (justification de fonctionnalités transversales).

**Critères de fin de phase** : toutes les règles non formalisées de la Phase 03 sont reprises et
statuées.

---

## Phase 12 — Intégrations

**Nom définitif proposé** : "Intégrations"

**Objectif** : Identifier les systèmes externes avec lesquels l'outil futur devra communiquer.

**Entité cible** : `InformationRegistre`.

**Exemple concret** : "Synchronisation avec le logiciel de comptabilité Sage, export mensuel
manuel suffisant pour l'instant — pas de temps réel requis."

**Questions principales** :
- Échange de données avec un système déjà existant ?
- Dans quel sens ?
- Automatique ou export/import manuel suffisant pour l'instant ?
- Accès technique déjà documenté (API, export possible) ?

**Questions de relance** :
- Reprendre les systèmes/outils déjà cités en Phase 07 et Phase 10.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 07, 10. Vers 13 (fréquence/disponibilité de synchronisation → exigence NF).

**Critères de fin de phase** : chaque système externe mentionné précédemment est traité ici.

---

## Phase 13 — Exigences non-fonctionnelles

**Nom définitif proposé** : "Exigences non-fonctionnelles"

**Objectif** : Capturer les exigences de qualité de l'outil (performance, disponibilité,
sécurité, accessibilité, volume).

**Entité cible** : `InformationRegistre`.

**Exemple concret** : "5 utilisateurs simultanés maximum, usage en horaires de bureau
uniquement, environ 200 nouveaux dossiers par an."

**Questions principales** :
- Combien de personnes utiliseront l'outil, à quelle fréquence ?
- Besoin de disponibilité particulier ?
- Contrainte de sécurité particulière (données sensibles, RGPD déjà évoqué en Phase 10) ?
- Besoin d'accès mobile / hors ligne ?
- Volume de données prévisible ?

**Questions de relance** :
- Toute contrainte technique/réglementaire (10) ou intégration (12) est reprise pour vérifier
  si elle implique une exigence non-fonctionnelle précise.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 10, 12. Vers 08 (justification de fonctionnalités transversales).

**Critères de fin de phase** : nombre d'utilisateurs et fréquence d'usage renseignés.

---

## Phase 14 — Priorisation MVP

**Nom définitif proposé** : "Priorisation MVP"

**Objectif** : Trancher définitivement la Priorite MoSCoW de chaque Fonctionnalite (jusqu'ici
`NonArbitree`), en s'appuyant sur les Problèmes quantifiés (Phase 04).

**Particularité** : ne crée pas de nouvelle entité — présente la liste des Fonctionnalites
(toutes encore `NonArbitree` à ce stade, sauf retour en arrière), triées par ScoreCalcule du
Probleme couvert (via LienTracabilite), et fait choisir Must/Should/Could/Won't pour chacune.
Une Fonctionnalite justifiée par une Contrainte/Règle/Exigence NF plutôt que par un Probleme
(cas des fonctionnalités transversales) est présentée séparément, avec sa propre justification
affichée au lieu d'un score.

**Exemple concret** : F-005 (couvre PROB-002, ScoreCalcule=45) → proposée en Must Have ;
F-012 "Export PDF personnalisable des devis" (couvre PROB-007, ScoreCalcule=4) → l'analyste
questionne si elle doit vraiment être Must Have.

**Questions principales** :
- Pour chaque Fonctionnalite : Must / Should / Could / Won't ?
- Le Probleme le plus critique (Phase 04) est-il couvert par au moins une Fonctionnalite Must
  Have ?

**Questions de relance** :
- Pour toute Fonctionnalite qu'on s'apprête à classer Must Have alors qu'elle couvre un
  Probleme de gravité Faible/Modéré : "Êtes-vous sûr que celle-ci doit être dans la première
  version ?"

**Questions bloquantes** :
- Au moins un Probleme critique doit être couvert par une Fonctionnalite Must Have.
- Aucune Fonctionnalite ne doit rester `NonArbitree` en fin de phase.

**Liens avec les autres phases** :
- Depuis 04, 08.

**Critères de fin de phase** : chaque Fonctionnalite a une Priorite MoSCoW explicitement
tranchée (plus aucune `NonArbitree`) · le Probleme le plus critique est couvert par au moins une
Fonctionnalite Must Have.

---

## Phase 15 — Planning

**Nom définitif proposé** : "Planning"

**Objectif** : Esquisser un séquencement réaliste des lots de développement.

**Entité cible** : `InformationRegistre`.

**Exemple concret** : "Lot 1 (Must Have) : gestion des devis + tableau de bord. Lot 2 (Should
Have) : automatisation des relances. Pas de contrainte de délai externe."

**Questions principales** :
- Échéance externe contraignante (reprend/confirme la Phase 10) ?
- Les Fonctionnalites Must Have (Phase 14) tiennent-elles en un seul lot, ou faut-il découper ?
- Dépendance connue entre certaines Fonctionnalites ?

**Questions de relance** :
- Si le délai (Phase 10) semble incompatible avec le volume Must Have (Phase 14) → signaler la
  tension explicitement.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** :
- Depuis 10, 14.

**Critères de fin de phase** : au moins un séquencement est explicitement formulé.

---

## Phase 16 — Synthèse *(inversée avec l'ancienne Phase 17 — correction v2)*

**Nom définitif proposé** : "Synthèse"

**Objectif** : Produire le résumé exécutif qui servira de support à la validation du décideur
en Phase 17 — **avant** la relecture formelle, pas après (correction de l'ordre v1, qui faisait
relire les registres bruts à un dirigeant de PME avant même qu'un résumé n'existe).

**Entité cible** : `InformationRegistre` — alimente directement `MarkdownExportService`
(génère un brouillon ANALYSE.md consultable).

**Exemple concret** : "Le Cabinet Verdier perd actuellement 3 jours par devis par manque de
visibilité sur les dossiers en attente (PROB-002, score 45). L'outil proposé introduit un
tableau de bord de suivi et automatise les relances de cotisations impayées. Périmètre MVP :
4 fonctionnalités Must Have, livrables en un seul lot. Point ouvert : contrat de reprise des
données Excel à confirmer avec le trésorier."

**Questions principales** :
- Résumé exécutif en 3-5 phrases : qui est le client, quel est le problème principal, quelle
  est la solution envisagée ?
- Les 3 problèmes les plus critiques (Phase 04) sont-ils bien reflétés ?
- Points ouverts ou risques majeurs à mentionner explicitement ?

**Questions de relance** :
- Si le résumé ne mentionne aucun problème critique (🔴) de la Phase 04 → à corriger.

**Questions bloquantes** : aucune.

**Liens avec les autres phases** : toutes. Vers 17 (ce résumé est le support présenté au
décideur pour validation) et 18 (alimente la section 1 du Prompt Maître de développement).

**Critères de fin de phase** : un résumé exécutif de 3-5 phrases existe, généré via
`MarkdownExportService`, et mentionne les problèmes critiques.

---

## Phase 17 — Validation *(inversée avec l'ancienne Phase 16 — correction v2)*

**Nom définitif proposé** : "Validation"

**Objectif** : Faire relire et valider formellement le **résumé produit en Phase 16** (pas les
16 registres bruts) par le décideur identifié en Phase 01.

**Entité cible** : `DecisionRegistre` (Description, Justification, AlternativesEcartees, Date).

**Exemple concret** : Description "Validation du périmètre MVP proposé", Justification "Le
budget de 8000€ permet de couvrir les 4 fonctionnalités Must Have identifiées",
AlternativesEcartees "Automatisation complète des relances écartée pour l'instant, coût jugé
disproportionné par rapport au gain".

**Questions principales** :
- Le décideur a-t-il relu la synthèse produite en Phase 16 ?
- Points de désaccord ou d'hésitation ?
- Pour chaque point discuté : quelle décision, pourquoi, quelles alternatives écartées ?

**Questions de relance** :
- Toute `QuestionRegistre` encore Bloquante et Ouverte à ce stade — provenant des questions
  bloquantes créées explicitement en 01-04/08/etc. (voir correction générale ci-dessous) — est
  reprise ici : "toujours pas résolue — la traiter maintenant ou l'acter comme risque assumé ?"

**Questions bloquantes** :
- Rappel de la règle déjà codée (section 4.3 du Prompt Maître) : le niveau de maturité ne peut
  pas atteindre 5 tant qu'il existe une QuestionRegistre Bloquante Ouverte.

**Liens avec les autres phases** : depuis 16 directement ; concerne transversalement le reste.

**Critères de fin de phase** : aucune question bloquante ouverte · au moins une Decision
formellement enregistrée actant la validation globale.

---

## Phase 18 — Transfert prompt maître de développement

**Nom définitif proposé** : "Transfert prompt maître" *(le nom de la Phase reste court dans
l'UI ; le terme complet "Prompt Maître de développement" est utilisé dans les libellés d'aide
pour éviter l'ambiguïté avec le Prompt Maître d'analyse de ce projet)*

**Objectif** : Vérifier que tout ce qui est nécessaire à la génération automatique du Prompt
Maître de développement (déjà implémentée, `PromptMaitreTransfertService`) est présent et
cohérent — une checklist de complétude, pas une nouvelle saisie.

**Entité cible** : aucune nouvelle.

**Questions principales** (checklist) :
- Demande et 3 problèmes principaux identifiés (section 1) ?
- Stack technique envisagée renseignée sur le Projet (section 2) ?
- Entites et Fonctionnalites à jour (section 4) ?
- Acteurs avec Permissions renseignées (section 9) ?
- Fonctionnalites triées par Priorite MoSCoW définitivement tranchée (section 10) ?

**Questions bloquantes** :
- Niveau de maturité à 5 requis (aucune question bloquante ouverte, aucune fonctionnalité
  orpheline au sens corrigé, aucun problème non couvert).

**Liens avec les autres phases** : toutes.

**Critères de fin de phase** : export "Transfert prompt maître" généré et relu, niveau de
maturité à 5.

---

## Correction générale — questions bloquantes 01-18 → QuestionRegistre explicite

**Point mineur v1 intégré** : dans toutes les phases (01 à 18), chaque question marquée
"bloquante" qui reste sans réponse satisfaisante en fin de phase doit créer explicitement une
`QuestionRegistre` (Importance = Bloquante, Statut = Ouverte) — pas seulement "être signalée"
de façon informelle. C'est cette QuestionRegistre que la Phase 17 (Validation) retrouve et fait
trancher, et c'est elle qui plafonne `NiveauMaturite` (section 4.3 du Prompt Maître, déjà codé
dans `MaturiteCalculatorService`). Ce point n'était pas explicite dans la v1 pour les Phases
01-04 alors que la mécanique en dépend déjà.

---

## Récapitulatif des noms de phases (v2)

| N° | Nom actuel (provisoire) | Nom proposé (v2) | Changement vs v1 |
|---|---|---|---|
| 5 | Acteurs (provisoire) | Acteurs | contenu modifié (permissions déplacées en 06) |
| 6 | Données (provisoire) | Données | contenu modifié (grille de permissions ajoutée) |
| 7 | Documents (provisoire) | Documents | inchangé |
| 8 | Fonctionnalités (provisoire) | Fonctionnalités | contenu modifié (orpheline, MoSCoW) |
| 9 | Automatisations (provisoire) | Automatisations | clarification ajoutée |
| 10 | Contraintes (provisoire) | Contraintes | inchangé |
| 11 | Règles métier (provisoire) | Règles métier | inchangé |
| 12 | Intégrations (provisoire) | Intégrations | inchangé |
| 13 | Non-fonctionnel (provisoire) | Exigences non-fonctionnelles | inchangé |
| 14 | Priorisation MVP (provisoire) | Priorisation MVP | contenu modifié (arbitrage MoSCoW) |
| 15 | Planning (provisoire) | Planning | inchangé |
| 16 | Validation (provisoire) | **Synthèse** | **inversé avec 17** |
| 17 | Synthèse (provisoire) | **Validation** | **inversé avec 16** |
| 18 | Transfert prompt maître (provisoire) | Transfert prompt maître | vocabulaire clarifié |

---

## Impact technique résumé (v2)

1. **Modèle de données** :
   - `LienTracabilite` : ajout d'un champ optionnel `InformationRegistreId` (référence
     Contrainte/Règle/Exigence NF), contrainte CHECK "au moins un lien" étendue.
   - `InformationRegistre` : ajout d'un lien polymorphe optionnel vers une entité du domaine
     (deux colonnes, ex. `EntiteType` string + `EntiteReferenceId` int) — porte Source/Statut
     pour les entités créées en Phases 05-09 (Option B).
   - `PrioriteMoSCoW` : ajout de la valeur `NonArbitree` (devient la valeur par défaut à la
     création d'une Fonctionnalite).
   - `Projet` : ajout de `NotesPreparation` (string?).
   - `ContradictionDetectorService` : étendu pour couvrir les InformationRegistre compagnons
     des entités 05-09 (même mécanique que pour 01-04, pas de nouveau service).
2. **Phases 05, 06 (fusionnées côté UX), 07, 08, 09** : chacune un composant Blazor dédié
   piloté par le mode entretien — création de l'entité + de son InformationRegistre compagnon
   pour Source/Statut ; Phase 06 inclut en plus la grille de permissions (`MudDataGrid`).
3. **Phases 10, 11, 12, 13, 15** : réutilisent le mécanisme actuel (`QuestionGuidee` →
   `InformationRegistre`), extension de `QuestionsGuideesParPhase`.
4. **Phase 14** : vue "liste + arbitrage" (tableau éditable des Fonctionnalites).
5. **Phase 16 (Synthèse)** : réutilise `QuestionGuidee` → `InformationRegistre`, avec un aperçu
   du brouillon `MarkdownExportService` intégré à la vue.
6. **Phase 17 (Validation)** : composant simple pour `DecisionRegistre`.
7. **Phase 18** : vue de checklist en lecture seule, aucun nouveau composant de saisie.
8. **Notes de préparation** : `Projet.NotesPreparation` + page Client dédiée, indépendante des
   Phases.
9. **`CreateProjetAction.cs`** : mise à jour des noms de phases 5-18 (retrait de
   "(provisoire)"), y compris l'inversion de contenu 16/17.

Rédaction v2 terminée. En attente de votre relecture avant tout code.
