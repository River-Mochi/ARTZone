// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Floating HUD button (GameTopLeft). On click, fires the trigger that
//          ZoningControllerToolUISystem handles to instantiate/toggle the tool.

import { FloatingButton } from "cs2/ui";
import React from "react";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";
import gridIcon from "../../images/grid-color.svg";

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ACT");
    return <FloatingButton onClick={onClickTopLeft} src={gridIcon} tooltipLabel={tooltip} />;
}
