// src/AdvancedRoadTools/UI/src/mods/ZoningToolSections.tsx
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
const uilStandard = "coui://uil/Standard/";
const allSrc = uilStandard + "StarAll.svg";

const RoadZoningMode$ = bindValue<number>(mod.id, 'RoadZoningMode');
const ToolZoningMode$ = bindValue<number>(mod.id, 'ToolZoningMode');
const isRoadPrefab$ = bindValue<boolean>(mod.id, 'IsRoadPrefab');

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

        // These get the value of the bindings.
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const isRoadPrefab = useValue(isRoadPrefab$);
        const zoningToolActive = useValue(tool.activeTool$).id == "Zone Controller Tool";
        const SelectedToolZoningMode = useValue(ToolZoningMode$) as ZoningMode;
        const SelectedRoadZoningMode = useValue(RoadZoningMode$) as ZoningMode;

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();

        const ZoningModeTitle = translate("ToolOptions.SECTION[AdvancedRoadTools.Zone_Controller.SectionTitle]", "Zoning Side");
        const ZoningModeBothTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeBothDescription]", "Zone on both sides.");
        const ZoningModeLeftTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]", "Zone only on the left side.");
        const ZoningModeRightTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]", "Zone only on the right side.");

        var result = Component();


        //Currently the mod doesn't work when placing roads, only with the Zoning TOol
        if (isRoadPrefab) {
            result.props.children?.push(
                <>
                    {(<VanillaComponentResolver.instance.Section title={ZoningModeTitle}>
                        <>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedRoadZoningMode & ZoningMode.Both) == ZoningMode.Both)}
                                tooltip={ZoningModeBothTooltipDescription}
                                onSelect={flipRoadBothMode}
                                src={all_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedRoadZoningMode & ZoningMode.Left) == ZoningMode.Left)}
                                tooltip={ZoningModeLeftTooltipDescription}
                                onSelect={() => changeRoadZoningMode(SelectedRoadZoningMode ^ ZoningMode.Left)}
                                src={left_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedRoadZoningMode & ZoningMode.Right) == ZoningMode.Right)}
                                tooltip={ZoningModeRightTooltipDescription}
                                onSelect={() => changeRoadZoningMode(SelectedRoadZoningMode ^ ZoningMode.Right)}
                                src={right_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                        </>
                    </VanillaComponentResolver.instance.Section>
                    )}
                </>
            )
        }
        else if (zoningToolActive) {
            result.props.children?.push(
                <>
                    {(<VanillaComponentResolver.instance.Section title={ZoningModeTitle}>
                        <>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedToolZoningMode & ZoningMode.Both) == ZoningMode.Both)}
                                tooltip={ZoningModeBothTooltipDescription}
                                onSelect={flipToolBothMode}
                                src={all_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedToolZoningMode & ZoningMode.Left) == ZoningMode.Left)}
                                tooltip={ZoningModeLeftTooltipDescription}
                                onSelect={() => changeToolZoningMode(SelectedToolZoningMode ^ ZoningMode.Left)}
                                src={left_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={((SelectedToolZoningMode & ZoningMode.Right) == ZoningMode.Right)}
                                tooltip={ZoningModeRightTooltipDescription}
                                onSelect={() => changeToolZoningMode(SelectedToolZoningMode ^ ZoningMode.Right)}
                                src={right_icon}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                        </>
                    </VanillaComponentResolver.instance.Section>
                    )}
                </>
            )
        }

        return result;
    }
}
