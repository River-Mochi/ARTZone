// File: src/UI/src/mods/ez-zone-tool-button.tsx
// Purpose:
//   Floating GameTopLeft (GTL) launcher button (icon + tooltip).
//   Clicking triggers ToggleZoneControllerTool on the C# side.
// Notes:
//   - Uses cs2/ui Button "floating" variant, with icon via the `src` prop.
//   - Keep the colored SVG on Button.src. Do NOT use Icon tinted={true} here;
//     tinting recolors the SVG and hides EZ's own icon colors.
//   - Uses onSelect (CS2 UI toolchain), not onClick.

import React from "react";
import { trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import mod from "mod.json";

import { VanillaComponentResolver } from "../components/VanillaComponentResolver";
import { ZONING_TOOL_ID } from "../shared/tool-ids";

// Color SVG path:
//   Button src={MainIconPath} lets the SVG keep its own colors.
//   Use Icon tinted={true} only for icons that are meant to become monochrome/white.
import MainIconPath from "../../images/ico-zones-color02.svg";

export default function EZZoneToolButton() {
    const { translate } = useLocalization();

    // Vanilla active-tool binding: this drives the GTL selected visual only.
    // It does not toggle the tool; handleSelect below sends that command to C#.
    const activeToolId = useValue(tool.activeTool$)?.id;
    const selected = activeToolId === ZONING_TOOL_ID;

    // Tooltip strings live in locale files; fallback text lives here.
    const title = translate("EasyZoning.Zone_Controller.ToolName", "Easy Zoning");
    const description = translate(
        "EasyZoning.Zone_Controller.ToolDescription",
        "This opens the EZ update roads panel.\nShortcut: Ctrl+V"
    );

    // UI only sends the trigger; C# side performs tool toggle + PhotoMode guard.
    const handleSelect = () => {
        trigger(mod.id, "ToggleZoneControllerTool");

        // Devtools trace (localhost:9444).
        try {
            console.log("[EZ][UI] GameTopLeft button → ToggleZoneControllerTool");
        } catch {
        }
    };

    // Vanilla tooltip component resolver (avoids importing private vanilla internals directly).
    const resolver = VanillaComponentResolver.instance;
    const DescriptionTooltip = resolver.DescriptionTooltip;


    // Button.src preserves EZ's colored SVG; for a white icon, use Icon tinted={true}.
    // selected gives the vanilla light-blue GTL active state when clicked on.
    // onSelect is CS2 UI handler; keeps GTL button independent and works if Ctrl+V fails.
    return (
        <DescriptionTooltip title={title} description={description} direction="right">
            <Button
                variant="floating"
                src={MainIconPath}
                selected={selected}
                onSelect={handleSelect}
            />
        </DescriptionTooltip>
    );
}
