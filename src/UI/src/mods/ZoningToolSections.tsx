// File: src/UI/src/mods/ZoningToolSections.tsx
// Purpose: Renders the “Zoning Side” 3-button group in the Tool Options panel.
// UI-only: Handles LMB on the three buttons. No world/road mouse input here.
//
// • Click Left/Right button sets that exact side.
// • Click “Both” button toggles Both <-> None (all highlighted or none highlighted).
// World interactions (hover preview, RMB flip, LMB apply) are implemented in C#
// ZoningControllerToolSystem and are independent from this file.

import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
import styles from "./ZoningToolSections.module.scss";

import { ZONING_TOOL_ID } from "../shared/tool-ids";

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

        const activeTool = useValue(tool.activeTool$)?.id;
        const isRoadPrefab = useValue(isRoadPrefab$);
        const zoningToolActive = activeTool === ZONING_TOOL_ID;

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const title = translate("ToolOptions.SECTION[ARTZone.Zone_Controller.SectionTitle]") || "Zoning Side";
        const tipBoth = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeBothDescription]") || "Toggle Both/None.";
        const tipLeft = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeLeftDescription]") || "Zone only the left side.";
        const tipRight = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeRightDescription]") || "Zone only the right side.";

        // Show under vanilla road placement OR under our tool
        if (isRoadPrefab || zoningToolActive) {
            const usingRoadState = isRoadPrefab;
            const selected = usingRoadState ? roadMode : toolMode;

            const onLeft = () => (usingRoadState ? setRoadZoningMode(ZoningMode.Left) : setToolZoningMode(ZoningMode.Left));
            const onRight = () => (usingRoadState ? setRoadZoningMode(ZoningMode.Right) : setToolZoningMode(ZoningMode.Right));
            const onBoth = () => (usingRoadState ? flipRoadBothMode() : flipToolBothMode());

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
