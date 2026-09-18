using MudBlazor;

namespace Client.Theme;

/// <summary>
/// Thème visuel Solideo Digital — repris de la charte du site vitrine
/// (solideo-digital/public/css/variables.css) : or #B8935E, bleu marine #1A2B3C,
/// beige #F5F0E8, police Inter.
///
/// L'or n'a un contraste WCAG suffisant (≥ 4.5:1) que sur fond clair pour du texte de taille
/// normale — jamais comme fond de texte blanc, ni comme couleur de texte sur blanc (ratio réel
/// 2.85:1). Il est donc réservé aux accents (bordures actives, icônes, highlights) ; le bleu
/// marine porte le texte et les surfaces d'action (contraste 14.4:1 sur blanc, 5.1:1 sur or).
/// </summary>
public static class SolideoDigitalTheme
{
    private const string Or = "#B8935E";
    private const string OrFonce = "#9A7A4A";
    private const string OrClair = "#D4B882";
    private const string Marine = "#1A2B3C";
    private const string MarineClair = "#2C3E50";
    private const string MarineFonce = "#0F1A26";
    private const string Beige = "#F5F0E8";

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Marine,
            PrimaryDarken = MarineFonce,
            PrimaryLighten = MarineClair,
            Secondary = Or,
            SecondaryDarken = OrFonce,
            SecondaryLighten = OrClair,
            // L'or n'a pas un contraste WCAG suffisant pour porter du texte blanc (2.85:1) :
            // le texte affiché automatiquement sur un fond Secondary (boutons, chips) doit
            // rester en marine (5.1:1 sur or) plutôt que le blanc par défaut du framework.
            SecondaryContrastText = Marine,
            Tertiary = Or,
            TertiaryContrastText = Marine,

            Background = Beige,
            Surface = "#FFFFFF",
            AppbarBackground = Marine,
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = Marine,
            DrawerIcon = Marine,

            TextPrimary = Marine,
            TextSecondary = "#4B5563",

            Success = "#10B981",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Info = "#3B82F6",

            LinesDefault = "#E5E7EB",
            TableLines = "#E5E7EB",
            Divider = "#E5E7EB",
        },

        PaletteDark = new PaletteDark
        {
            Primary = OrClair,
            Secondary = Or,
            Tertiary = OrClair,

            Background = "#12181F",
            Surface = "#1A2330",
            AppbarBackground = MarineFonce,
            AppbarText = "#FFFFFF",
            DrawerBackground = "#1A2330",
            DrawerText = "#F5F0E8",
            DrawerIcon = OrClair,

            TextPrimary = "#F5F0E8",
            TextSecondary = "#B8C0CC",

            Success = "#10B981",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Info = "#3B82F6",
        },

        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "-apple-system", "Segoe UI", "Roboto", "sans-serif"] },
            H1 = new H1Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "700" },
            H2 = new H2Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "700" },
            H3 = new H3Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "600" },
            H4 = new H4Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "600" },
            H5 = new H5Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "600" },
            H6 = new H6Typography { FontFamily = ["Inter", "sans-serif"], FontWeight = "600" },
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
