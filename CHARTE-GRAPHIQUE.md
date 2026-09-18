# Charte graphique Solideo Digital

Référence de marque à réutiliser telle quelle sur tout projet Solideo Digital, quelle que soit
la stack (Blazor/MudBlazor, Laravel/Blade, autre). Source de vérité : le site vitrine
(`solideo-digital/public/css/variables.css`) — ce document en est la synthèse indépendante de
toute techno, à copier dans le contexte d'un nouveau projet pour lui donner le même thème.

## Couleurs

### Palette de marque

| Rôle | Nom | Hex |
|---|---|---|
| Or (accent principal, logo) | `color-gold` | `#B8935E` |
| Or clair | `color-gold-light` | `#D4B882` |
| Or foncé | `color-gold-dark` | `#9A7A4A` |
| Marine (couleur porteuse, texte, surfaces d'action) | `color-navy` | `#1A2B3C` |
| Marine clair | `color-navy-light` | `#2C3E50` |
| Marine foncé | `color-navy-dark` | `#0F1A26` |
| Beige (fond clair) | `color-beige` | `#F5F0E8` |

**Règle de contraste WCAG — la plus importante de cette charte :** l'or n'a **pas** un contraste
suffisant (2.85:1) pour porter du texte sur fond blanc, ni pour du texte blanc sur fond or. Il
est réservé aux **accents** : bordures actives, icônes, titres de section sur fond sombre,
highlights au survol. Le marine porte le texte et les surfaces d'action (contraste 14.4:1 sur
blanc, 5.1:1 sur or) — c'est lui qui doit dominer visuellement, l'or ponctue.

### Neutres

| Nom | Hex |
|---|---|
| `color-white` | `#FFFFFF` |
| `color-gray-50` | `#F9FAFB` |
| `color-gray-100` | `#F3F4F6` |
| `color-gray-200` | `#E5E7EB` |
| `color-gray-300` | `#D1D5DB` |
| `color-gray-400` | `#9CA3AF` |
| `color-gray-500` | `#6B7280` |
| `color-gray-600` | `#4B5563` |
| `color-gray-700` | `#374151` |
| `color-gray-800` | `#1F2937` |
| `color-gray-900` | `#111827` |

### Couleurs de statut

| Rôle | Hex |
|---|---|
| Succès | `#10B981` |
| Avertissement | `#F59E0B` |
| Erreur | `#EF4444` |
| Info | `#3B82F6` |

## Typographie

- **Police principale (sans-serif)** : `Inter`, avec la pile de secours
  `-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif`.
  Chargée depuis Google Fonts si non disponible en local.
- **Police secondaire (serif, usage ponctuel)** : `Georgia`, secours `"Times New Roman", serif`.
- Titres (`h1`-`h6`) en `Inter`, poids 600-700 selon le niveau.

## Espacements

Échelle en `rem`, à respecter pour garder une cohérence de rythme visuel entre projets :

| Nom | Valeur |
|---|---|
| `spacing-xs` | `0.5rem` |
| `spacing-sm` | `1rem` |
| `spacing-md` | `1.5rem` |
| `spacing-lg` | `2rem` |
| `spacing-xl` | `3rem` |
| `spacing-2xl` | `4rem` |

## Bordures (radius)

| Nom | Valeur |
|---|---|
| `border-radius-sm` | `0.25rem` |
| `border-radius-md` | `0.5rem` |
| `border-radius-lg` | `1rem` |
| `border-radius-xl` | `1.5rem` |
| `border-radius-full` | `9999px` |

## Ombres

| Nom | Valeur |
|---|---|
| `shadow-sm` | `0 1px 2px 0 rgba(0, 0, 0, 0.05)` |
| `shadow-md` | `0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)` |
| `shadow-lg` | `0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)` |
| `shadow-xl` | `0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)` |

## Transitions

| Nom | Valeur |
|---|---|
| `transition-fast` | `150ms ease-in-out` |
| `transition-normal` | `300ms ease-in-out` |
| `transition-slow` | `500ms ease-in-out` |

## Logo

- Fichiers sources : `solideo-digital/public/images/` (logo complet, favicon).
- Ce projet utilise une version simplifiée en tant que mark d'app bar :
  `Client/wwwroot/images/logo-mark-sd.png`.

## Footer

Style bandeau bas de page, sobre : fond marine, texte beige, mention or en accent, liens
soulignés/dorés au survol. Voir la section « Footer » de la documentation de référence
(`solideo-digital/resources/views/layouts/app.blade.php`) pour la version marketing complète du
site vitrine (4 colonnes : présentation, services, liens rapides, contact).

Pour un outil **interne** (pas de vocation commerciale), n'en reprendre que le style visuel et
une version minimale du contenu — pas de liens Portfolio/Blog/Services qui n'ont pas de sens hors
contexte vitrine. Voir `Client/Layout/Footer.razor` dans ce projet pour un exemple d'application
de cette règle.

## Application dans un projet Blazor / MudBlazor

Ce projet contient une implémentation prête à copier :

- `Client/Theme/SolideoDigitalTheme.cs` — thème `MudTheme` complet (palettes claire/sombre,
  typographie), directement copiable dans un nouveau projet MudBlazor.
- `Client/Layout/Footer.razor` — composant de bandeau bas de page, à adapter au contenu du
  nouveau projet.

## Application dans un projet non-Blazor

Reprendre `solideo-digital/public/css/variables.css` tel quel comme fichier de design tokens CSS
(custom properties) — il est indépendant de Laravel et s'utilise dans n'importe quel projet web.
