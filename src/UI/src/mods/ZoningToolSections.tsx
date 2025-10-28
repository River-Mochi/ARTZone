// File: src/UI/src/mods/ZoningToolSections.tsx

// Purpose: Renders the “Zoning Side” 3-button group in the Tool Options panel.

import type { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
import { ZONING_TOOL_ID } from "../shared/tool-ids";

// Icons (emitted to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab"); // “should show section”

function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
    try { console.log("[EZ][UI] setToolZoningMode →", value); } catch { }
}
function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
    try { console.log("[EZ][UI] setRoadZoningMode →", value); } catch { }
}
function flipRoadBothMode() { trigger(mod.id, "FlipRoadBothMode"); try { console.log("[EZ][UI] flipRoadBothMode"); } catch { } }
function flipToolBothMode() { trigger(mod.id, "FlipToolBothMode"); try { console.log("[EZ][UI] flipToolBothMode"); } catch { } }

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const result = Component(props);

        const activeToolId = useValue(tool.activeTool$)?.id;
        const showSection = useValue(isRoadPrefab$);     // show when our tool OR a Road prefab is active
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const title = translate("ToolOptions.SECTION[EasyZoning.Zone_Controller.SectionTitle]", "Zoning Side");
        const tipBoth = translate("ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeBothDescription]", "Toggle Both/None.");
        const tipLeft = translate("ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeLeftDescription]", "Zone only the left side.");
        const tipRight = translate("ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeRightDescription]", "Zone only the right side.");

        if (showSection) {
            // If a Road prefab is active AND our tool is not, use roadMode; otherwise use toolMode.
            const usingRoadState = !zoningToolOn && showSection;
            const selected = usingRoadState ? roadMode : toolMode;

            const onLeft = () => usingRoadState ? setRoadZoningMode(ZoningMode.Left) : setToolZoningMode(ZoningMode.Left);
            const onRight = () => usingRoadState ? setRoadZoningMode(ZoningMode.Right) : setToolZoningMode(ZoningMode.Right);
            const onBoth = () => usingRoadState ? flipRoadBothMode() : flipToolBothMode();

            // Push our section/buttons into vanilla MouseToolOptions
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={onBoth}
                            src={IconBoth}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={onLeft}
                            src={IconLeft}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={onRight}
                            src={IconRight}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        }

        return result;
    };
};
