// File: src/UI/src/mods/ZoningToolSections.tsx
// Purpose: MouseToolOptions section - draws the triple buttons (Both/Left/Right),
//          reads state via bindValue, and talks to C# via bindings triggers.
//
// Behavior:
//  • When a vanilla road prefab is active -> operate RoadZoningMode (NEW road placement).
//  • When our tool is active              -> operate ToolZoningMode (EXISTING roads).
//  • LMB on Left/Right sets that exact side.
//  • LMB on Both toggles Both <-> None (so “Both” highlights all three; “None” highlights none).
//  • RMB flipping on roads (Left <-> Right or Both<->None until LMB confirms) is implemented in C#
//    ZoningControllerToolSystem; this file mirrors the chosen mode and updates on LMB only.

// ----  Zoning Tool Sections (MouseToolOptions) ------------------------------
// DO NOT CHANGE ZONING_TOOL_ID – must match C# ZoningControllerToolSystem.ToolID.

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


const ZONING_TOOL_ID = "ARTZone.ZoningTool";    // DO NOT CHANGE – must equal C# ZoningControllerToolSystem.ToolID

function setToolZoningMode(value: ZoningMode) {
    try { console.log(`[ART][UI] TS click: ChangeToolZoningMode(${ZoningMode[value]}=${value})`); } catch { }
    trigger(mod.id, "ChangeToolZoningMode", value);
}
function setRoadZoningMode(value: ZoningMode) {
    try { console.log(`[ART][UI] TS click: ChangeRoadZoningMode(${ZoningMode[value]}=${value})`); } catch { }
    trigger(mod.id, "ChangeRoadZoningMode", value);
}
function flipRoadBothMode() {
    try { console.log("[ART][UI] TS click: FlipRoadBothMode()"); } catch { }
    trigger(mod.id, "FlipRoadBothMode");
}
function flipToolBothMode() {
    try { console.log("[ART][UI] TS click: FlipToolBothMode()"); } catch { }
    trigger(mod.id, "FlipToolBothMode");
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const result = Component(props);

        const active = useValue(tool.activeTool$);
        const activeToolId = active?.id ?? "";
        const isRoadPrefab = !!useValue(isRoadPrefab$);
        const zoningToolActive = activeToolId === ZONING_TOOL_ID;

        const toolMode = (useValue(ToolZoningMode$) ?? ZoningMode.Both) as ZoningMode;
        const roadMode = (useValue(RoadZoningMode$) ?? ZoningMode.Both) as ZoningMode;

        try { console.log(`[ART][UI] render: activeToolId=${activeToolId} isRoadPrefab=${isRoadPrefab} zoningToolActive=${zoningToolActive} toolMode=${toolMode} roadMode=${roadMode}`); } catch { }

        const { translate } = useLocalization();
        const title = translate("ToolOptions.SECTION[ARTZone.Zone_Controller.SectionTitle]") || "Zoning Side";
        const tipBoth = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeBothDescription]") || "Toggle Both/None (Both highlights all three).";
        const tipLeft = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeLeftDescription]") || "Zone only left side.";
        const tipRight = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeRightDescription]") || "Zone only right side.";

        if (isRoadPrefab || zoningToolActive) {
            const usingRoadState = isRoadPrefab;
            const selected = usingRoadState ? roadMode : toolMode;

            const onLeft = () => usingRoadState ? setRoadZoningMode(ZoningMode.Left) : setToolZoningMode(ZoningMode.Left);
            const onRight = () => usingRoadState ? setRoadZoningMode(ZoningMode.Right) : setToolZoningMode(ZoningMode.Right);
            const onBoth = () => usingRoadState ? flipRoadBothMode() : flipToolBothMode();

            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={onBoth}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={onLeft}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={onRight}
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
