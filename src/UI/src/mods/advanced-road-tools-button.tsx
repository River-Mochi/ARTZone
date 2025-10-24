// File: src/UI/src/mods/advanced-road-tools-button.tsx
// Purpose: Show the main ART button in GameTopLeft (floating). Uses the same SVG the tool tile uses.
// Runtime path comes from webpack publicPath ("coui://ui-mods/") + asset generator ("images/[name][ext]").

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

import gridIconUrl from "../../images/grid-road.svg";

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ZoningToolControllerButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("AdvancedRoadTools.Zone_Controller.ToolName", "ART");
    return (
        <Button
            variant="floating"
            src={gridIconUrl}        // -> resolves to "coui://ui-mods/images/grid-road.svg"
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
