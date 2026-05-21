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
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Protections" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Raccourcis clavier" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilité" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Visuels" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "UTILISATION" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Empêcher la suppression des bâtiments" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Bâtiments = cellules occupées**. Empêche l’aperçu/application de condamner des bâtiments.\n\n" +
                    "**[ ✓ ] Activation recommandée.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Empêcher la réinitialisation des carrés déjà peints/zonés" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Ne réinitialise pas les cellules déjà zonées pendant l’aperçu/application.\n\n" +
                    "**[ ✓ ] Activation recommandée.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Panneau EZ On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Raccourci clavier** pour afficher rapidement le panneau Easy Zoning\n" +
                    "**par défaut Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "Courbes de niveau" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "Afficher le bouton" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] activé**, affiche le bouton Courbes de niveau dans le panneau de mise à jour des routes existantes.\n\n" +
                    "● Désactivez ceci si vous préférez un panneau plus petit ou si un autre mod gère les courbes de niveau." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Panneau verre" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] activé**, utilise un style translucide plus lisible pour le panneau.\n" +
                    "**[   ] désactivé** = panneau gris.\n\n" +
                    "<Style visuel uniquement.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Couleur de bordure : suppressions en aperçu" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Couleur de bordure pour l’aperçu des cellules à supprimer.\n\n" +
                    "<Orange> = plus vif et plus facile à voir.\n" +
                    "<Rouge> = contraste rouge plus fort.\n" +
                    "<Rose> = couleur vive et fun.\n" +
                    "<Violet> = contraste doux mais visible.\n" +
                    "<Rouge vanilla> = correspond à l’apparence par défaut du jeu." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Opacité de la bordure" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Ajuste l’opacité de la bordure de l’aperçu de suppression.\n\n" +
                    "<100%> garde la transparence normale de l’aperçu.\n" +
                    "<0%> masque la bordure." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Couleur de remplissage : suppressions en aperçu" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Style de couleur de remplissage pour l’aperçu des cellules pouvant être supprimées.\n\n" +
                    "<Rouge vanilla> = apparence actuelle du jeu.\n" +
                    "<Blanc> = contraste plus net.\n" +
                    "<Orange> = correspond à la bordure orange.\n" +
                    "<Rose> = couleur vive et fun.\n" +
                    "<Violet> = contraste doux mais visible.\n" +
                    "<Aucun> = bordure seule, minimaliste" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Opacité du remplissage" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Ajuste l’opacité du remplissage pour l’aperçu des cellules supprimables.\n\n" +
                    "<100%> garde la transparence normale de l’aperçu.\n" +
                    "<0%> masque le remplissage.\n" +
                    "Ignoré si <Remplissage suppression> est réglé sur <Aucun>." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "Contraste élevé" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "Préréglage :\n" +
                    "<Panneau verre On>\n" +
                    "<Bordure orange>\n" +
                    "<Opacité de bordure 100%>\n" +
                    "<Aucun remplissage.>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "Couleur du jeu" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "Utilise la bordure et le remplissage rouges comme l’aperçu de zonage du jeu." },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "Orange" },
                { "EasyZoning.Dropdown.Color.Red", "Rouge" },
                { "EasyZoning.Dropdown.Color.Pink", "Rose" },
                { "EasyZoning.Dropdown.Color.Purple", "Violet" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "Rouge vanilla" },
                { "EasyZoning.Dropdown.Color.White", "Blanc" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "Aucun (bordure seule)" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Afficher les instructions" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Afficher ou masquer les **instructions d’utilisation** ci-dessous." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Routes existantes>\n" +
                    "1. Ouvrez le panneau EZ Update : cliquez sur <Ctrl+V> pour activer/désactiver le panneau\n" +
                    "   (<icône en haut à gauche> fait la même chose).\n" +
                    "2. Utilisez les 3 icônes EZ pour Deux côtés / Gauche / Droite.\n" +
                    "   Cliquez à nouveau sur le bouton pour Aucun.\n" +
                    "3. Survolez une route pour prévisualiser.\n" +
                    "4. Aperçu rouge = cellules à supprimer.\n" +
                    "5. <RMB fait défiler> : Deux côtés → Gauche → Droite → Aucun → ...\n" +
                    "6. <LMB une fois> : applique (verrouille le choix).\n" +
                    "7. <Maintenir LMB + glisser> sur plusieurs sections de route, relâcher pour appliquer.\n" +
                    "8. <Annuler :> éloignez la souris et relâchez **LMB**.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = clic droit, <LMB> = clic gauche\n" +
                    "-----------------------------------------\n\n" +
                    "<Nouvelle route>\n" +
                    "1. Ouvrez le panneau Routes (choisissez une route).\n" +
                    "2. En bas du panneau d’outil de route : utilisez les 3 icônes EZ pour Deux côtés / Gauche / Droite.\n" +
                    "   Cliquez à nouveau sur le bouton sélectionné pour Aucun.\n" +
                    "3. Dessinez normalement.\n\n" +
                    "-------------------------------------------\n" +
                    "<Bouton terrain>\n" +
                    "<◎ Courbes de niveau> affiche les lignes d’élévation du terrain."
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Cycle classique au clic droit" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Non recommandé**\n" +
                    "OFF signifie : RMB fait défiler les 4 modes : **Deux côtés → Gauche → Droite → Aucun → ...**\n\n" +
                    "Avantage : moins besoin de ramener la souris vers le panneau d’outil.\n\n" +
                    "<-------------------------------------->\n" +
                    "Si Classique est ON : RMB bascule dans deux groupes séparés et demande plus de mouvements de souris :\n" +
                    "Gauche ↔ Droite seulement\n" +
                    "Deux côtés ↔ Aucun seulement"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Basculer le panneau de mise à jour Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom affiché de ce mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Ouvrir la page Paradox Mods de l’auteur." },
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
