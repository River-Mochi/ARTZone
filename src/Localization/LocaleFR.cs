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
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),     "Comportement hérité de l’outil" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Ne pas réinitialiser les cases déjà zonées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Ne réinitialise pas les cellules déjà zonées pendant la prévisualisation / l’application.\n\n" +
                    "**[ ✓ ] Activation recommandée.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des bâtiments" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Bâtiments = cellules occupées**. Empêche la prévisualisation / l’application de nouveaux zones de transformer des bâtiments existants en condamnés.\n\n" +
                    "**[ ✓ ] Activation recommandée.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Afficher le panneau de mise à jour" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Afficher le panneau Easy Zoning (**Ctrl+Z par défaut**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Cycle RMB hérité" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Désactivé recommandé.**\n" +
                    "Lorsqu’il est désactivé, le RMB (clic droit) peut parcourir les 4 modes :\n" +
                    "Deux côtés → Gauche → Droite → Aucun → ...\n\n" +
                    "Avantage : plus rapide, moins besoin de revenir au panneau avec la souris.\n\n" +

                    "**Activé :** le RMB bascule entre deux ensembles séparés :\n" +
                    "Gauche ↔ Droite\n" +
                    "Deux côtés ↔ Aucun"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning – Basculer le panneau" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Modifier le zonage : deux côtés, gauche<->droite, ou aucun.\n" +
                    "Clic gauche confirme le choix. Maintenir clic gauche + glisser le long d’une route pour mettre à jour plusieurs segments." },

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
