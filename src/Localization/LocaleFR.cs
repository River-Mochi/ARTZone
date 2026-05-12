// File: src/Localization/LocaleFR.cs
// Purpose: French (fr-FR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleFR(Setting setting)
        {
            m_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;
            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title
                { m_Settings.GetSettingsLocaleID(), title },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Classique" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Options de zonage" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Raccourcis clavier" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilité" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Interface" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "UTILISATION" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Ne pas réinitialiser les cases déjà zonées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Ne réinitialise pas les cellules déjà zonées pendant l'aperçu/l'application.\n\n" +
                    "**[ ✓ ] Activé recommandé.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des bâtiments" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Les bâtiments = cellules occupées**. Empêche l'aperçu/l'application de nouveaux zonages de transformer des bâtiments existants en bâtiments condamnés.\n\n" +
                    "**[ ✓ ] Activé recommandé.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Afficher/Masquer le panneau de mise à jour" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Afficher le panneau Easy Zoning (**Ctrl+V par défaut**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Bouton Contour" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] activé**, affiche le bouton Contour dans le panneau Easy Zoning pour les routes existantes.\n\n" +
                    "Désactiver si un autre mod gère déjà les courbes de niveau du terrain." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Style de panneau transparent" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] activé**, utilise un panneau translucide plus clair.\n" +
                    "**[   ] désactivé**, utilise un panneau plus sombre de style vanilla.\n\n" +
                    "Style visuel uniquement. Aucun flou n'est utilisé." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)), "◉ Orange remove-preview edge" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)),
                    "**[ ✓ ] enabled**, use a brighter orange border for cells that will be removed.\n" +
                    "**[   ] disabled**, keep the vanilla red border.\n\n" +
                    "Only changes the remove-preview border. Fill stays vanilla for now." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Remove-preview edge opacity" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Adjusts only the orange remove-preview border opacity.\n\n" +
                    "Does not change normal zoning colors or the white add-preview cells." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Afficher les instructions" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Afficher ou masquer les **instructions d'utilisation** ci-dessous." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nouvelle route>\n" +
                    "1. Ouvrir le panneau Routes (choisir une route).\n" +
                    "2. En bas du panneau d'outil route : utilisez les 3 icônes EZ pour Deux côtés / Gauche / Droite.\n" +
                    "   Cliquez à nouveau sur le bouton sélectionné pour Aucun.\n" +
                    "3. Tracer la route normalement.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = clic droit, LMB = clic gauche\n" +
                    "-----------------------------------------\n\n" +
                    "<Route existante>\n" +
                    "1. Ouvrir le panneau EZ de mise à jour : cliquer sur <Ctrl+V> pour afficher/masquer le panneau\n" +
                    "   (ou <l'icône en haut à gauche> fait la même chose).\n" +
                    "2. Utilisez les 3 icônes EZ pour Deux côtés / Gauche / Droite.\n" +
                    "   Cliquez à nouveau sur le bouton sélectionné pour Aucun.\n" +
                    "3. Survoler + prévisualiser une route.\n" +
                    "4. L'aperçu rouge = cellules qui seront supprimées.\n" +
                    "5. <RMB fait défiler> : Deux côtés → Gauche → Droite → Aucun → ...\n" +
                    "6. <LMB une fois> : applique (valide).\n" +
                    "7. <Maintenir LMB + glisser> sur plusieurs segments de route, puis relâcher pour appliquer.\n" +
                    "8. <Annuler :> éloigner la souris et relâcher **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<BOUTON OPTIONNEL>\n" +
                    "• <Contour> affiche les lignes d'altitude du terrain." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Cycle classique par clic droit" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF recommandé** pour que RMB fasse défiler les 4 modes :\n" +
                    "**Deux côtés → Gauche → Droite → Aucun → ...**\n\n" +
                    "Avantage : moins besoin de ramener la souris vers le panneau d'outil.\n\n" +
                    "--------------------------------------\n" +
                    "Si le mode classique est ON : RMB alterne entre deux ensembles séparés :\n" +
                    "Gauche ↔ Droite\n" +
                    "Deux côtés ↔ Aucun" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Afficher/Masquer le panneau Easy Zoning de mise à jour" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom affiché de ce mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Ouvrir la page Paradox Mods de l'auteur." },
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
