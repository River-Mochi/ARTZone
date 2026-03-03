// File: src/UI/src/mods/ez-zone-tool-button.tsx
// Purpose: Floating GameTopLeft launcher button (icon + tooltip).
//          Triggers ToggleZoneControllerTool on the C# side.

import React from "react";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "mod.json";

import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

// Icon emitted by webpack to coui://ui-mods/images/
import MainIconPath from "../../images/ico-zones-color02.svg";

export default function EZZoneToolButton() {
    const { translate } = useLocalization();
// Tooltip on GTL button.
    const title = translate("EasyZoning.Zone_Controller.ToolName", "Easy Zoning");
    const description = translate(
        "EasyZoning.Zone_Controller.ToolDescription",
        "This opens the EZ update roads panel.\nShortcut: Ctrl+Z"
    );

    const handleClick = () => {
        trigger(mod.id, "ToggleZoneControllerTool");
        try {
            console.log("[EZ][UI] GameTopLeft button → ToggleZoneControllerTool");
        } catch {
            // Ignore console failures
        }
    };

    const resolver = VanillaComponentResolver.instance;
    const DescriptionTooltip = resolver.DescriptionTooltip;

    return (
        <DescriptionTooltip title={title} description={description} direction="right">
            <Button variant="floating" onClick={handleClick}>
                <img src={MainIconPath} />
            </Button>
        </DescriptionTooltip>
    );
}
