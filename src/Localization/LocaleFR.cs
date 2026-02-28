// File: src/Localization/LocaleFR.cs
// Purpose: French (fr-FR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

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
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de zonage" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Ne pas réinitialiser les cases déjà zonées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Ne pas réinitialiser les cellules déjà zonées pendant l’aperçu/l’application.\n\n" +
                "**[ ✓ ] Activé recommandé.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des bâtiments" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Bâtiments = cellules occupées**. Empêche l’aperçu/l’application de nouvelles zones de rendre les bâtiments existants condamnés.\n\n" +
                "**[ ✓ ] Activé recommandé.**" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Afficher/Masquer le panneau" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Afficher le bouton du panneau Easy Zoning (Ctrl+Z par défaut)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Afficher/Masquer le panneau du bouton Easy Zoning" },

                // Legacy Panel (Road Services tile)
                //{ $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                //{ $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                //  "Choose zoning for roads: both, left, right, or none.\nRight-click flips; left-click applies." },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Modifier le zonage : des deux côtés, gauche<->droite, ou aucun.\n" +
                    "Clic gauche applique. Glisser le long d’une route pour mettre à jour plusieurs segments." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom d’affichage de ce mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Ouvrir la page Paradox Mods de l’auteur." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Rejoindre le Discord du mod." },
            };
            return d;
        }

        public void Unload( )
        {
        }
    }
}
