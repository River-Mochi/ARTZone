// File: src/UI/src/mods/ZoneGameTopButton.tsx
// Purpose: Floating HUD button to toggle the Zone Controller Tool.
// File: src/UI/src/mods/advanced-road-tools-button.tsx
import { FloatingButton } from "cs2/ui";
import React from "react";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Import so webpack emits to coui://ui-mods/images/ToolsIcon.png
import buttonIcon from "../../images/Tool_Icon/ToolsIcon.png";
// Keep we want this for GameTop Right icon
// import zoneIcon from "../../images/ZoneControllerTool.svg";

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
    const buttonTooltip = translate("Assets.NAME[Zone Controller Tool]", "Zone Controller");
    return (
        <FloatingButton
            onClick={ToggleZoneControllerTool}
            src={buttonIcon}                 // ← use the PNG (emitted by webpack)
            tooltipLabel={buttonTooltip}
        />
    );
}

export default ZoningToolControllerButton;
