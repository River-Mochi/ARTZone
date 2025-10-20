// File: src/UI/src/mods/ZoningToolSections.tsx
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

// Keep a local TS enum (cannot import C# into TS)
export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");

function changeToolZoningMode(z: ZoningMode) { trigger(mod.id, "ChangeToolZoningMode", z); }
function changeRoadZoningMode(z: ZoningMode) { trigger(mod.id, "ChangeRoadZoningMode", z); }
function flipRoadBothMode() { trigger(mod.id, "FlipRoadBothMode"); }
function flipToolBothMode() { trigger(mod.id, "FlipToolBothMode"); }

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const { children, ..._otherProps } = props || {};

        const activeTool = useValue(tool.activeTool$);
        const netToolActive = activeTool.id === tool.NET_TOOL;
        const zoningToolActive = activeTool.id === "Zone Controller Tool";
        const SelectedToolMode = useValue(ToolZoningMode$) as ZoningMode;
        const SelectedRoadMode = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const title = translate("ToolOptions.SECTION[ARTZone.Zone_Controller.SectionTitle]", "Zoning Side");
        const tipBoth = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeBothDescription]", "Zone on both sides.");
        const tipLeft = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeLeftDescription]", "Zone only on the left side.");
        const tipRight = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeRightDescription]", "Zone only on the right side.");

        function ensureTool() {
            if (!zoningToolActive) trigger(mod.id, "ToggleZoneControllerTool");
        }

        const result = Component();

        if (netToolActive) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoadMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={() => changeRoadZoningMode(SelectedRoadMode ^ ZoningMode.Both)}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoadMode & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={() => changeRoadZoningMode(SelectedRoadMode ^ ZoningMode.Left)}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoadMode & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={() => changeRoadZoningMode(SelectedRoadMode ^ ZoningMode.Right)}
                            src={right_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        }

        if (zoningToolActive) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedToolMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={() => { ensureTool(); flipToolBothMode(); }}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedToolMode & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={() => { ensureTool(); changeToolZoningMode(SelectedToolMode ^ ZoningMode.Left); }}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedToolMode & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={() => { ensureTool(); changeToolZoningMode(SelectedToolMode ^ ZoningMode.Right); }}
                            src={right_icon}
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
