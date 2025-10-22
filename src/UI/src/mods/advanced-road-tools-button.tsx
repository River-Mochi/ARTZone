// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Floating HUD button in GameTopLeft that toggles the Zone Controller Tool.
// NOTE: We use a direct import so webpack emits the asset to coui://ui-mods/images/grid-color.svg.

import { FloatingButton } from "cs2/ui";
import React from "react";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// === Icon for the GameTopLeft button ===
// Using a direct import keeps things simple and robust with your current webpack config.
import gridIcon from "../../images/grid-color.svg";

import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";

function ToggleZoneControllerTool() {
    // Toggle the same mini panel the hotkey opens
    trigger(mod.id, "ToggleMiniPanel");
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
            onClick={ToggleZoneControllerTool}   // this calls trigger(mod.id, "ToggleMiniPanel")
            src={gridIcon}                      // ← now uses grid-color.svg
            tooltipLabel={buttonTooltip}
        />
    );
}
export default ZoningToolControllerButton;
