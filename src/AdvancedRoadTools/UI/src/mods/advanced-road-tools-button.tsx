// UI/src/mods/advanced-road-tools-button.tsx

import { FloatingButton } from "cs2/ui";
import React from "react";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Use the PNG for the main tool button (palette/floating button look)
import buttonIcon from "../../images/Tool_Icon/ToolsIcon.png";

// (No change here; this just fires our binder in the C# UI system)
function ToggleZoneControllerTool() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

// Optional: if you use the vanilla tooltip shell elsewhere
export function descriptionTooltip(
    tooltipTitle: string | null,
    tooltipDescription: string | null
): JSX.Element {
    return (
        <>
            <div>{tooltipTitle}</div>
            <div>{tooltipDescription}</div>
        </>
    );
}

function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const buttonTooltip = translate(
        "AdvancedRoadTools.Zone_Controller.ToolName",
        "Zone Controller"
    );

    return (
        <FloatingButton
            onClick={ToggleZoneControllerTool}
            src={buttonIcon}
            tooltipLabel={buttonTooltip}
        />
    );
}

export default ZoningToolControllerButton;
