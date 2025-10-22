// src/UI/src/mods/ZoningToolSections.tsx
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
import styles from "./ZoningToolSections.module.scss";
import all_icon from "../../images/Toolbar/all/ico-all.svg";
import left_icon from "../../images/Toolbar/left/ico-left.svg";
import right_icon from "../../images/Toolbar/right/ico-right.svg";

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");
const ShowMiniPanel$ = bindValue<boolean>(mod.id, "ShowMiniPanel");


function changeToolZoningMode(zoningMode: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", zoningMode);
}

function changeRoadZoningMode(zoningMode: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", zoningMode);
}

function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
}
function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const { children, ...otherProps } = props || {};

        const activeTool = useValue(tool.activeTool$).id;
        const isRoadPrefab = useValue(isRoadPrefab$);
        const zoningToolActive = activeTool === "Zone Controller Tool";
        const SelectedToolZoningMode = useValue(ToolZoningMode$) as ZoningMode;
        const SelectedRoadZoningMode = useValue(RoadZoningMode$) as ZoningMode;
        const showMini = useValue(ShowMiniPanel$);


        // Only keys, no hardcoded English. Tiny dev fallback with || so it doesn't render empty if key missing.
        const { translate } = useLocalization();
        const ZoningModeTitle =
            translate("ToolOptions.SECTION[AdvancedRoadTools.Zone_Controller.SectionTitle]") ||
            "Zoning Side";
        const ZoningModeBothTooltipDescription =
            translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeBothDescription]") ||
            "Zone both sides.";
        const ZoningModeLeftTooltipDescription =
            translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]") ||
            "Zone only the left side.";
        const ZoningModeRightTooltipDescription =
            translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]") ||
            "Zone only the right side.";

        const result = Component();

        // Show when:
        //  • vanilla road tool active (isRoadPrefab), or
        //  • our tool active (zoningToolActive), or
        //  • the mini panel is toggled on (showMini).
        if (isRoadPrefab || zoningToolActive || showMini) {
            const useRoadState = isRoadPrefab; // when under vanilla tool we operate RoadZoningMode

            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={ZoningModeTitle}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={((useRoadState ? SelectedRoadZoningMode : SelectedToolZoningMode) & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={ZoningModeBothTooltipDescription}
                            onSelect={useRoadState ? flipRoadBothMode : flipToolBothMode}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={((useRoadState ? SelectedRoadZoningMode : SelectedToolZoningMode) & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={ZoningModeLeftTooltipDescription}
                            onSelect={() =>
                                useRoadState
                                    ? changeRoadZoningMode(SelectedRoadZoningMode ^ ZoningMode.Left)
                                    : changeToolZoningMode(SelectedToolZoningMode ^ ZoningMode.Left)
                            }
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={((useRoadState ? SelectedRoadZoningMode : SelectedToolZoningMode) & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={ZoningModeRightTooltipDescription}
                            onSelect={() =>
                                useRoadState
                                    ? changeRoadZoningMode(SelectedRoadZoningMode ^ ZoningMode.Right)
                                    : changeToolZoningMode(SelectedToolZoningMode ^ ZoningMode.Right)
                            }
                            src={right_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        }

        return result;
    };
};
