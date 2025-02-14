import {ModuleRegistryExtend} from "cs2/modding";
import {bindValue, trigger, useValue} from "cs2/api";
import {game, tool} from "cs2/bindings";
import {useLocalization} from "cs2/l10n";
import mod from "../../mod.json";
import {VanillaComponentResolver} from "../YenYang/VanillaComponentResolver";
import styles from "./ZoningToolSections.module.scss";
import { Icon } from "cs2/ui";
import Thumbnail from "../../ZoneControllerTool.svg";

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3
}
const uilStandard =                         "coui://uil/Standard/";
const gameStandard =                           "Media/Tools/";
const arrowDownSrc =                uilStandard +  "ArrowDownThickStroke.svg";
const arrowUpSrc =                  uilStandard +  "ArrowUpThickStroke.svg";

const ZoningMode$ = bindValue<number>(mod.id, 'ZoningMode');
const ZoningDepthLeft$ = bindValue<number>(mod.id, 'ZoningDepthLeft');
const ZoningDepthRight$ = bindValue<number>(mod.id, 'ZoningDepthRight');

const depthDownLeftID = "depth-down-left-arrow";
const depthUpLeftID = "depth-up-left-arrow";
const depthDownRightID = "depth-down-right-arrow";
const depthUpRightID = "depth-up-right-arrow";

// This functions trigger an event on C# side and C# designates the method to implement.
function handleClick(eventName: string)
{
    trigger(mod.id, eventName);
}

function changeZoningMode(zoningMode: ZoningMode) {
    trigger(mod.id, "ChangeZoningMode", zoningMode);
}

export function descriptionTooltip(tooltipTitle: string | null, tooltipDescription: string | null): JSX.Element {
    return (
        <>
            <div className={VanillaComponentResolver.instance.descriptionTooltipTheme.title}>{tooltipTitle}</div>
            <div
                className={VanillaComponentResolver.instance.descriptionTooltipTheme.content}>{tooltipDescription}</div>
        </>
    );
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const zoningToolActive = useValue(tool.activeTool$).id == "Zone Controller Tool";
        const ZoningDepthLeft = useValue(ZoningDepthLeft$);
        const ZoningDepthRight = useValue(ZoningDepthRight$);
        const SelectedZoningMode = useValue(ZoningMode$) as ZoningMode;

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const {translate} = useLocalization();

        const ZoningModeTitle = translate("ToolOptions.TOOLTIP_TITLE[AdvancedRoadTools.Zone_Controller.ZoningModeTitle]", "Zoning Side");
        const ZoningDepthTitle = translate("ToolOptions.TOOLTIP_TITLE[AdvancedRoadTools.Zone_Controller.ZoningDepthTitle]", "Zoning Depth");
        const ZoningDepthDownTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningDepthDownTooltipDescription]", "Decreases by one cell.");
        const ZoningDepthUpTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningDepthUpTooltipDescription]", "Increases by one cell.");
        const ZoningModeLeftTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeLeftDescription]", "Change the zoning depth on the left side.");
        const ZoningModeRightTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[AdvancedRoadTools.Zone_Controller.ZoningModeRightDescription]", "Change the zoning depth on the right side.");

        var result = Component();
        
        //Currently the mod doesn't work when placing roads, only with the Zoning TOol
        //if (netToolActive || zoningToolActive) {
        if (zoningToolActive) {
            result.props.children?.push(
                <>
                    {(<VanillaComponentResolver.instance.Section title={ZoningModeTitle}>
                            <>
                                <VanillaComponentResolver.instance.ToolButton
                                    selected={((SelectedZoningMode & ZoningMode.Left) == ZoningMode.Left)}
                                    tooltip={ ZoningModeLeftTooltipDescription}
                                    onSelect={() => changeZoningMode(SelectedZoningMode ^ ZoningMode.Left)}
                                    //src={}                                                
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}>L</label></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton
                                    selected={((SelectedZoningMode & ZoningMode.Right) == ZoningMode.Right)}
                                    tooltip={ZoningModeRightTooltipDescription}
                                    onSelect={() => changeZoningMode(SelectedZoningMode ^ ZoningMode.Right)}
                                    //src={}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label className={styles.centeredContentButton}>R</label></VanillaComponentResolver.instance.ToolButton>
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