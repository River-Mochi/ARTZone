// UI/src/mods/advanced-road-tools-button.tsx

import { FloatingButton } from "cs2/ui";
import React from "react";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Button image (this renders on the floating button)
import buttonIcon from "../../images/Tool_Icon/ToolsIcon.png";

// If you later use the SVG in this file, import it like this:
// import zoneIcon from "../../images/Toolbar/ZoneControllerTool.svg";

import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";

function ToggleZoneControllerTool() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export function descriptionTooltip(
    tooltipTitle: string | null,
    tooltipDescription: string | null
): JSX.Element {
    return (
        <>
            <div className={VanillaComponentResolver.instance.descriptionTooltipTheme.title}>
                {tooltipTitle}
            </div>
            <div className={VanillaComponentResolver.instance.descriptionTooltipTheme.content}>
                {tooltipDescription}
            </div>
        </>
    );
}

function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const buttonTooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ACT");

    return (
        <FloatingButton
            onClick={ToggleZoneControllerTool}
            src={buttonIcon}
            tooltipLabel={buttonTooltip}
        />
    );
}

export default ZoningToolControllerButton;
