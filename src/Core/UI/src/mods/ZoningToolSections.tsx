import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue }  from "cs2/api";
import { game, tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
import styles from "./ZoningToolSections.module.scss";

export enum ZoningMode{
    None = 0,
    Right = 1,
    Left = 2,
    Both = 4
}

const ZoningMode$ = bindValue<number>(mod.id, 'ZoningMode');
const ZoningDepth$ = bindValue<number>(mod.id, 'ZoningDepth');

const zoningDownID = "zoning-down-arrow";
const zoningUpID = "zoning-up-arrow";

function changeZoningMode(zoningMode: ZoningMode) {
    trigger(mod.id, "ChangeZoningMode", zoningMode);
}

function changeZoningDepth(depth: number) {
    trigger(mod.id, "ChangeZoningDepth", depth);
}

export function descriptionTooltip(tooltipTitle: string | null, tooltipDescription: string | null) : JSX.Element {
    return (
        <>
            <div className={VanillaComponentResolver.instance.descriptionTooltipTheme.title}>{tooltipTitle}</div>
            <div className={VanillaComponentResolver.instance.descriptionTooltipTheme.content}>{tooltipDescription}</div>
        </>
    );
}

export const ZoningToolController: ModuleRegistryExtend = (Component : any) => {
    return (props) =>{
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const zoningToolActive = useValue(tool.activeTool$).id == "Zoning Controller";
        const ZoningDepth = useValue(ZoningDepth$);
        const SelectedZoningMode = useValue(ZoningMode$) as ZoningMode;
        
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        
        const ZoningDepthTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Create]", "Place one");
        const ZoningDepthTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[Create]", "Place an individual item on the map.");
        const ZoningModeTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Brush]", "Zoning Side");
        const ZoningModeNoneTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Brush]", "Remove Zoning");
        const ZoningModeNoneTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[Brush]", "Removes zoning from both sides.");
        const ZoningModeBothTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Brush]", "Change zoning - Both Sides");
        const ZoningModeBothTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[Brush]", "Change the zoning depth on both sides");
        const ZoningModeLeftTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Brush]", "Change zoning - Left");
        const ZoningModeLeftTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[Brush]", "Change the zoning depth only on the left side.");
        const ZoningModeRightTooltipTitle = translate("ToolOptions.TOOLTIP_TITLE[Brush]", "Change zoning - Right");
        const ZoningModeRightTooltipDescription = translate("ToolOptions.TOOLTIP_DESCRIPTION[Brush]", "Change the zoning depth only on the right side.");
        
        var result = Component();
        
        if(netToolActive || zoningToolActive){
            result.props.children?.push(
                <>
                    { (<VanillaComponentResolver.instance.Section title={ZoningModeTooltipTitle}>
                            <VanillaComponentResolver.instance.ToolButton  
                                selected={(SelectedZoningMode & ZoningMode.Both) == ZoningMode.Both}     
                                tooltip={descriptionTooltip(ZoningModeBothTooltipTitle,ZoningModeBothTooltipDescription)}         
                                onSelect={() => changeZoningMode(ZoningMode.Both)}    
                                //src={}                                                
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     
                                className={VanillaComponentResolver.instance.toolButtonTheme.button}>B</VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton 
                                selected={(SelectedZoningMode & ZoningMode.Left) == ZoningMode.Left}    
                                tooltip={descriptionTooltip(ZoningModeLeftTooltipTitle,ZoningModeLeftTooltipDescription)}
                                onSelect={() => changeZoningMode(ZoningMode.Left)}
                                //src={}                                                
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     
                                className={VanillaComponentResolver.instance.toolButtonTheme.button}>L</VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={(SelectedZoningMode & ZoningMode.Right) == ZoningMode.Right}
                                tooltip={descriptionTooltip(ZoningModeRightTooltipTitle,ZoningModeRightTooltipDescription)}
                                onSelect={() => changeZoningMode(ZoningMode.Right)}
                                //src={}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.button}>R</VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton
                                selected={(SelectedZoningMode & ZoningMode.None) == ZoningMode.None}
                                tooltip={descriptionTooltip(ZoningModeNoneTooltipTitle,ZoningModeNoneTooltipDescription)}
                                onSelect={() => changeZoningMode(ZoningMode.None)}
                                //src={}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                className={VanillaComponentResolver.instance.toolButtonTheme.button}>0</VanillaComponentResolver.instance.ToolButton>
                        </VanillaComponentResolver.instance.Section>
                    )}
                </>
            )
        }
        
        return result;
    }
}