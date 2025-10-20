// File: src/UI/src/mods/ZoningToolSections.tsx
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
// import styles from "./ZoningToolSections.module.scss"; // (unused)

// Local icons
import all_icon from "../../images/Toolbar/all/ico-all.svg";
import left_icon from "../../images/Toolbar/left/ico-left.svg";
import right_icon from "../../images/Toolbar/right/ico-right.svg";

// Bit flags must match C# enum semantics
export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");

function changeToolZoningMode(z: ZoningMode) { trigger(mod.id, "ChangeToolZoningMode", z); }
function changeRoadZoningMode(z: ZoningMode) { trigger(mod.id, "ChangeRoadZoningMode", z); }
function flipRoadBothMode() { trigger(mod.id, "FlipRoadBothMode"); }
function flipToolBothMode() { trigger(mod.id, "FlipToolBothMode"); }

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const result = Component();

        const activeToolId = useValue(tool.activeTool$).id;
        const netToolActive = activeToolId == tool.NET_TOOL;
        const zoningActive = activeToolId == "Zone Controller Tool";
        const isRoadPrefab = useValue(isRoadPrefab$);

        const SelectedTool = useValue(ToolZoningMode$) as ZoningMode;
        const SelectedRoad = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const Title = translate("ToolOptions.SECTION[ARTZone.Zone_Controller.SectionTitle]", "Zoning Side");
        const ZBoth = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeBothDescription]", "Zone on both sides.");
        const ZLeft = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeLeftDescription]", "Zone only the left side.");
        const ZRight = translate("ToolOptions.TOOLTIP_DESCRIPTION[ARTZone.Zone_Controller.ZoningModeRightDescription]", "Zone only the right side.");

        // 1) Vanilla Net Tool with an actual road prefab selected → show RoadZoningMode section.
        if (netToolActive && isRoadPrefab) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={Title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoad & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={ZBoth}
                            onSelect={flipRoadBothMode}    // explicit Both <-> None
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoad & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={ZLeft}
                            onSelect={() => changeRoadZoningMode((SelectedRoad ^ ZoningMode.Left) as ZoningMode)}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedRoad & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={ZRight}
                            onSelect={() => changeRoadZoningMode((SelectedRoad ^ ZoningMode.Right) as ZoningMode)}
                            src={right_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        }

        // 2) Our standalone tool active → show ToolZoningMode section.
        if (zoningActive) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={Title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedTool & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={ZBoth}
                            onSelect={flipToolBothMode}    // explicit Both <-> None
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedTool & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={ZLeft}
                            onSelect={() => changeToolZoningMode((SelectedTool ^ ZoningMode.Left) as ZoningMode)}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(SelectedTool & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={ZRight}
                            onSelect={() => changeToolZoningMode((SelectedTool ^ ZoningMode.Right) as ZoningMode)}
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
