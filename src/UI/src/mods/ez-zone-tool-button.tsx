// File: src/UI/src/mods/ez-zone-tool-button.tsx
// Purpose:
//   Floating GameTopLeft launcher button (icon + tooltip).
//   - Hidden in Photo Mode (clean screenshots).
//   - Click triggers ToggleZoneControllerTool on the C# side.

import React from "react";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "mod.json";

import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

// Icon emitted by webpack to coui://ui-mods/images/
import MainIconPath from "../../images/ico-zones-color02.svg";

// C# binding (ZoningControllerToolUISystem exposes IsPhotoMode)
const IsPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

export default function EZZoneToolButton() {
    // Hide the GTL button in Photo Mode for clean screenshots.
    const photoMode = useValue(IsPhotoMode$) === true;
    if (photoMode) return null;

    const { translate } = useLocalization();

    // Tooltip strings come from lang/*.json
    const title = translate("EasyZoning.Zone_Controller.ToolName", "Easy Zoning");
    const description = translate(
        "EasyZoning.Zone_Controller.ToolDescription",
        "This opens the EZ update roads panel.\nShortcut: Ctrl+Z"
    );

    const handleClick = () => {
        // C# side listens for this trigger.
        trigger(mod.id, "ToggleZoneControllerTool");
    };

    // Use vanilla DescriptionTooltip (same pattern as ZoneTools)
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
