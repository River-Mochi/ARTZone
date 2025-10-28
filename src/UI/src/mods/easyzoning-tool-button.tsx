// File: src/UI/src/mods/easyzoning-tool-button.tsx

// Purpose: Floating GameTopLeft launcher button (icon + tooltip). Triggers ToggleZoneControllerTool.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Menu top left Matches C# EasyZoningMod.MainIconPath → coui://ui-mods/images/ico-zones-color02.svg
import MainIconPath from "../../images/ico-zones-color02.svg";

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function EasyZoningToolButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("EasyZoning.Zone_Controller.ToolName", "Easy Zoning");
    return (
        <Button
            variant="floating"
            src={MainIconPath}
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
