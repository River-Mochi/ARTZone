// File: src/Settings/LocaleFR.cs
// French (fr-FR) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleFR(Setting setting) => s_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART — Zonage" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de zonage" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Ne pas supprimer les cellules déjà zonées" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "N'écrase pas les cellules déjà zonées pendant l’aperçu ou l’application.\n <Activation recommandée.>" },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Ne pas supprimer les cellules occupées" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "N'écrase pas les cellules déjà occupées pendant l’aperçu ou l’application (ex. bâtiments).\n <Activation recommandée.>" },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Afficher / masquer le panneau" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Affiche le panneau ART-Zone (par défaut Shift+Z)." },

                // Binding title in the keybinding dialog
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Afficher / masquer le panneau ART-Zone" },

                // Palette (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Modificateur de zonage" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Modifier le zonage des routes : les deux côtés, gauche, droite ou aucun. Clic droit inverse le choix. Clic gauche confirme." },

                // About tab labels
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom affiché de ce mod." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },
#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Version d'information" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Version + informations de build" },
#endif
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Ouvrir la page Paradox Mods." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Rejoindre le Discord du mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
