// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Top-left floating button that uses the SAME icon path as C# (single source of truth).
// Reads the path from the binding "MainIconPath" exposed by ZoningControllerToolUISystem.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { bindValue, useValue, trigger } from "cs2/api";
import mod from "../../mod.json";

// Binding provided by ZoningControllerToolUISystem (see snippet below)
// single source truth in Mod.cs
const mainIconPath$ = bindValue<string>(mod.id, "MainIconPath");

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ART");

    // Fallback keeps dev builds happy if the binding isn't ready yet.
    const iconSrc =
        useValue(mainIconPath$) || "coui://ui-mods/images/grid-road.svg";

    return (
        <Button
            variant="floating"
            src={iconSrc}
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
