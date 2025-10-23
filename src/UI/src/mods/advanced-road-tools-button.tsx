// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Color SVG in GameTopLeft (no monochrome mask).
// Change: Use <Button variant="floating"> instead of <FloatingButton>.

import React from "react";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Prefer a COUI URL so it's not re-processed as a mask by the wrapper.
const GRID_ICON = "coui://ui-mods/images/grid-color.svg";

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ART");
    return (
        <Button
            variant="floating"
            src={GRID_ICON}
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
