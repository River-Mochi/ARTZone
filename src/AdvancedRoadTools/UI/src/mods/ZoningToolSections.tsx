import {ModuleRegistryExtend} from "cs2/modding";
import {bindValue, trigger, useValue } from "cs2/api";
import {tool} from "cs2/bindings";
import {useLocalization} from "cs2/l10n";
import mod from "../../mod.json";
import {VanillaComponentResolver} from "../YenYang/VanillaComponentResolver";
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
const uilStandard =                         "coui://uil/Standard/";
const allSrc =              uilStandard + "StarAll.svg";

const ZoningMode$ = bindValue<number>(mod.id, 'ZoningMode');
const isRoadPrefab$ = bindValue<boolean>(mod.id, 'IsRoadPrefab');

function changeZoningMode(zoningMode: ZoningMode) {
    trigger(mod.id, "ChangeZoningMode", zoningMode);
}

function flipBothMode(){
    trigger(mod.id, "FlipBothMode");
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const isRoadPrefab = useValue(isRoadPrefab$);
        const zoningToolActive = useValue(tool.activeTool$).id == "Zone Controller Tool";
        const SelectedZoningMode = useValue(ZoningMode$) as ZoningMode;

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const {translate} = useLocalization();

        const ZoningModeTitle = translate("ToolOptions.SECTION[AdvancedRoadTools.Zone_Controller.SectionTitle]", "Zoning Side");
        const ZoningModeBothTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeBothDescription]", "Zone on both sides.");
        const ZoningModeLeftTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]", "Zone only on the left side.");
        const ZoningModeRightTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]", "Zone only on the right side.");

        var result = Component();
        
        
        //Currently the mod doesn't work when placing roads, only with the Zoning TOol
        if (isRoadPrefab || zoningToolActive) {
        //if (zoningToolActive) {
            result.props.children?.push(
                <>
                    {(<VanillaComponentResolver.instance.Section title={ZoningModeTitle}>
                            <>
                                <VanillaComponentResolver.instance.ToolButton
                                    selected={((SelectedZoningMode & ZoningMode.Both) == ZoningMode.Both)}
                                    tooltip={ZoningModeBothTooltipDescription}
                                    onSelect={flipBothMode}
                                    src={all_icon}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton
                                    selected={((SelectedZoningMode & ZoningMode.Left) == ZoningMode.Left)}
                                    tooltip={ ZoningModeLeftTooltipDescription}
                                    onSelect={() => changeZoningMode(SelectedZoningMode ^ ZoningMode.Left)}
                                    src={left_icon}                                                
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton
                                    selected={((SelectedZoningMode & ZoningMode.Right) == ZoningMode.Right)}
                                    tooltip={ZoningModeRightTooltipDescription}
                                    onSelect={() => changeZoningMode(SelectedZoningMode ^ ZoningMode.Right)}
                                    src={right_icon}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>
                            </>
                        </VanillaComponentResolver.instance.Section>
                    )}
                    {/*{((SelectedZoningMode & ZoningMode.Left) == ZoningMode.Left) && (*/}
                    {/*    <VanillaComponentResolver.instance.Section title={`Left ${ZoningDepthTitle}`}>*/}
                    {/*        <VanillaComponentResolver.instance.ToolButton*/}
                    {/*            tooltip={ZoningDepthDownTooltipDescription} */}
                    {/*            onSelect={() => handleClick(depthDownLeftID)} */}
                    {/*            src={arrowDownSrc} */}
                    {/*            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} */}
                    {/*            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton}></VanillaComponentResolver.instance.ToolButton>*/}
                    {/*        <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ ZoningDepthLeft }</div>*/}
                    {/*        <VanillaComponentResolver.instance.ToolButton */}
                    {/*            tooltip={ZoningDepthUpTooltipDescription}*/}
                    {/*            onSelect={() => handleClick(depthUpLeftID)}*/}
                    {/*            src={arrowUpSrc} */}
                    {/*            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} */}
                    {/*            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton}></VanillaComponentResolver.instance.ToolButton>*/}
                    {/*    </VanillaComponentResolver.instance.Section>*/}
                    {/*)}*/}
                    {/*{((SelectedZoningMode & ZoningMode.Right) == ZoningMode.Right) && (*/}
                    {/*    <VanillaComponentResolver.instance.Section title={`Right ${ZoningDepthTitle}`}>*/}
                    {/*        <VanillaComponentResolver.instance.ToolButton*/}
                    {/*            tooltip={ZoningDepthDownTooltipDescription}*/}
                    {/*            onSelect={() => handleClick(depthDownRightID)}*/}
                    {/*            src={arrowDownSrc}*/}
                    {/*            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}*/}
                    {/*            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton}></VanillaComponentResolver.instance.ToolButton>*/}
                    {/*        <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ ZoningDepthRight }</div>*/}
                    {/*        <VanillaComponentResolver.instance.ToolButton*/}
                    {/*            tooltip={ZoningDepthUpTooltipDescription}*/}
                    {/*            onSelect={() => handleClick(depthUpRightID)}*/}
                    {/*            src={arrowUpSrc}*/}
                    {/*            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}*/}
                    {/*            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton}></VanillaComponentResolver.instance.ToolButton>*/}
                    {/*    </VanillaComponentResolver.instance.Section>*/}
                    {/*)}*/}
                </>
            )
        }

        return result;
    }
}