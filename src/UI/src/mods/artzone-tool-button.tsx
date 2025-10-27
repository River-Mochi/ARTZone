// File: src/UI/src/mods/artzone-tool-button.tsx
// Purpose: self-contained floating GameTopLeft button (launch/toggl tool/panel and shows icon + tooltip).
// Uses the SAME SVG file name as C# MainIconPath points to.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "../../mod.json";

// Keep the identifier name aligned with C#: MainIconPath (TS side uses a URL string)
import MainIconPath from "../../images/ico-4square-color.svg";     // TopLeft button

function onClickTopLeft() {
    trigger(mod.id, "ToggleZoneControllerTool");
}

export default function ARTZoneToolButton(): JSX.Element {
    const { translate } = useLocalization();
    const tooltip = translate("ARTZone.Zone_Controller.ToolName", "ART-Zone");
    return (
        <Button
            variant="floating"
            src={MainIconPath} // resolves to coui://ui-mods/images/ico-4square-color.svg or whatever image is MainIconPath
            onClick={onClickTopLeft}
            tooltipLabel={tooltip}
        />
    );
}
