// File: src/Settings/LocaleFR.cs
// French (fr-FR) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleFR(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — Zonage" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de zonage" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Ne pas supprimer les cellules déjà zonées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "N'écrase pas les cellules déjà zonées pendant l’aperçu ou l’application.\n <Activation recommandée.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Ne pas supprimer les cellules occupées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "N'écrase pas les cellules déjà occupées pendant l’aperçu ou l’application (ex. bâtiments).\n <Activation recommandée.>" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Afficher / masquer le panneau" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Affiche le panneau ART-Zone (par défaut Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Afficher / masquer le panneau ART-Zone" },

                // Palette (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Modificateur de zonage" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Modifier le zonage des routes : les deux côtés, gauche, droite ou aucun. Clic droit inverse le choix. Clic gauche confirme." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom affiché de ce mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Ouvrir la page Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Rejoindre le Discord du mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
