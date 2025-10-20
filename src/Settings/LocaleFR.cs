// File: src/LocaleFR.cs
/// <summary>
/// French locale (fr-FR)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleFR(Setting setting)
        {
            s_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups (Actions tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de l’outil de zonage" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },

                // Groups (About tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Liens" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Empêcher la suppression des zones existantes" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Empêche le remplacement des zones existantes pendant l’aperçu ou l’application." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des cellules occupées" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Empêche le remplacement des cellules contenant des bâtiments pendant l’aperçu ou l’application." },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Bouton de souris pour inverser le zonage" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Associez un bouton de souris pour inverser la direction du zonage lorsque l’outil est actif." },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Inverser le zonage" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Contrôleur de zone" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Contrôle le comportement du zonage le long d’une route.\nChoisissez entre les deux côtés, gauche, droite ou aucun.\nClic droit pour inverser la configuration." },

                // About tab link buttons
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Ouvrir la page Paradox Mods dans votre navigateur." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Ouvrir le Discord ART-Zone dans votre navigateur." },
            };
        }

        public void Unload()
        {
        }
    }
}
