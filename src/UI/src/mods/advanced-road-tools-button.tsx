// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Color SVG in GameTopLeft. Use <Button variant="floating"> to avoid masking.
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

import gridIconUrl from "../../images/grid-color.svg";

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ART");
    return (
        <Button
            variant="floating"
            src={gridIconUrl}
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
