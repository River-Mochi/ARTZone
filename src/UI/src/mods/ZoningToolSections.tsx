// File: src/UI/src/mods/ZoningToolSections.tsx
// Purpose: Inject a “Zoning Side” section with 3 EXCLUSIVE buttons into MouseToolOptions.
// Behavior:
//  • When a vanilla road prefab is active -> operate RoadZoningMode (NEW road placement).
//  • When our tool is active             -> operate ToolZoningMode (EXISTING roads).
//  • Buttons are EXCLUSIVE: clicking Left means EXACTLY Left (no XOR).
//  • Both button sets EXACTLY Both.

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
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");

function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
}
function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
}
function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
}
function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const result = Component(props);
        const activeTool = useValue(tool.activeTool$).id;
        const isRoadPrefab = useValue(isRoadPrefab$);
        const zoningToolActive = activeTool === "Zone Controller Tool";

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const title =
            translate("ToolOptions.SECTION[AdvancedRoadTools.Zone_Controller.SectionTitle]") ||
            "Zoning Side";
        const tipBoth =
            translate(
                "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeBothDescription]"
            ) || "Zone both sides.";
        const tipLeft =
            translate(
                "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]"
            ) || "Zone only the left side.";
        const tipRight =
            translate(
                "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]"
            ) || "Zone only the right side.";

        // Show under vanilla road placement OR under our tool
        if (isRoadPrefab || zoningToolActive) {
            const usingRoadState = isRoadPrefab;
            const selected = usingRoadState ? roadMode : toolMode;

            // Helpers to set EXCLUSIVE values (no XOR)
            const setLeft = () =>
                usingRoadState ? setRoadZoningMode(ZoningMode.Left) : setToolZoningMode(ZoningMode.Left);
            const setRight = () =>
                usingRoadState ? setRoadZoningMode(ZoningMode.Right) : setToolZoningMode(ZoningMode.Right);
            const setBoth = () =>
                usingRoadState ? flipRoadBothMode() : flipToolBothMode(); // keep Both <-> None flip if you want; or replace with exact Both:
            // To make Both to be EXACT Both (no None toggle), replace previous line with:
            // usingRoadState ? setRoadZoningMode(ZoningMode.Both) : setToolZoningMode(ZoningMode.Both);

            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={setBoth}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Left) === ZoningMode.Left && selected !== ZoningMode.Both}
                            tooltip={tipLeft}
                            onSelect={setLeft}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Right) === ZoningMode.Right && selected !== ZoningMode.Both}
                            tooltip={tipRight}
                            onSelect={setRight}
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
