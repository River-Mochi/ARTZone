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
        private readonly Setting m_Setting;
        public LocaleFR(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups (Actions tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de l’outil de zonage" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },

                // Groups (About tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Liens" },

                // Toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Empêcher la suppression des zones existantes" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Empêche le remplacement des zones existantes pendant l’aperçu ou l’application." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des cellules occupées" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Empêche le remplacement des cellules contenant des bâtiments pendant l’aperçu ou l’application." },

                // Keybind
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Bouton de souris pour inverser le zonage" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Associez un bouton de souris pour inverser la direction du zonage lorsque l’outil est actif." },
                { m_Setting.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Inverser le zonage" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Contrôleur de zone" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Contrôle le comportement du zonage le long d’une route.\nChoisissez entre les deux côtés, gauche, droite ou aucun.\nClic droit pour inverser la configuration." },

                // About tab link buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Ouvrir la page Paradox Mods dans votre navigateur." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Ouvrir le Discord ART-Zone dans votre navigateur." },
            };
        }

        public void Unload()
        {
        }
    }
}
