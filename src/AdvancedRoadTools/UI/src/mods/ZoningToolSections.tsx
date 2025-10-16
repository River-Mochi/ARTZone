// UI/src/mods/ZoningToolSections.tsx
// Injects zoning buttons (Both/Left/Right) into the vanilla tool options panel.

import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
import styles from "./ZoningToolSections.module.scss";

// Toolbar icons (built by webpack from UI/images/Toolbar/**)
import all_icon from "../../images/Toolbar/all/ico-all.svg";
import left_icon from "../../images/Toolbar/left/ico-left.svg";
import right_icon from "../../images/Toolbar/right/ico-right.svg";

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3
}

// Settings/UI bindings exposed by the C# UISystem
const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");

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

// The extension that injects our section into the vanilla panel
export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const { children, ...otherProps } = props || {};

        // Current vanilla tool state
        const activeTool = useValue(tool.activeTool$);
        const netToolActive = activeTool?.id === tool.NET_TOOL;
        const zoningToolActive = activeTool?.id === "Zone Controller Tool"; // must match C# ToolID

        // Our binding values
        const isRoadPrefab = useValue(isRoadPrefab$);
        const selectedToolZoningMode = useValue(ToolZoningMode$) as ZoningMode;
        const selectedRoadZoningMode = useValue(RoadZoningMode$) as ZoningMode;

        // Localization (falls back to provided strings if keys are missing)
        const { translate } = useLocalization();
        const sectionTitle = translate(
            "ToolOptions.SECTION[AdvancedRoadTools.Zone_Controller.SectionTitle]",
            "Zoning Side"
        );
        const bothDesc = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeBothDescription]",
            "Zone on both sides."
        );
        const leftDesc = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]",
            "Zone only on the left side."
        );
        const rightDesc = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]",
            "Zone only on the right side."
        );

        // Render the original component first
        const result = Component(otherProps);

        // Only show our section when the relevant tool/prefab is active
        if (isRoadPrefab) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={sectionTitle} key="art-road-zoning">
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedRoadZoningMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={bothDesc}
                            onSelect={flipRoadBothMode}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedRoadZoningMode & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={leftDesc}
                            onSelect={() => changeRoadZoningMode(selectedRoadZoningMode ^ ZoningMode.Left)}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedRoadZoningMode & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={rightDesc}
                            onSelect={() => changeRoadZoningMode(selectedRoadZoningMode ^ ZoningMode.Right)}
                            src={right_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        } else if (zoningToolActive) {
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={sectionTitle} key="art-tool-zoning">
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedToolZoningMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={bothDesc}
                            onSelect={flipToolBothMode}
                            src={all_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedToolZoningMode & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={leftDesc}
                            onSelect={() => changeToolZoningMode(selectedToolZoningMode ^ ZoningMode.Left)}
                            src={left_icon}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}
                        >
                            <label className={styles.centeredContentButton}></label>
                        </VanillaComponentResolver.instance.ToolButton>

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selectedToolZoningMode & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={rightDesc}
                            onSelect={() => changeToolZoningMode(selectedToolZoningMode ^ ZoningMode.Right)}
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
