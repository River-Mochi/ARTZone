// File: src/UI/src/mods/easyzoning-tool-button.tsx
// Purpose: Floating GameTopLeft launcher button (icon + tooltip). Triggers ToggleZoneControllerTool.
// Notes:
//   • CS2 Button types use `onPress` (not onClick) in current builds.
//   • Avoid explicit JSX.Element return type to sidestep editor "Cannot find namespace 'JSX'" if TS libs are off.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
// Use the webpack alias so this works from any folder depth:
import mod from "mod.json";

import MainIconPath from "../../images/ico-zones-color02.svg";

export default function EasyZoningToolButton() {
    const { translate } = useLocalization();
    const tooltip = translate("EasyZoning.Zone_Controller.ToolName", "Easy Zoning");
    const onPress = () => trigger(mod.id, "ToggleZoneControllerTool");

    return (
        <Button variant="floating" src={MainIconPath} onPress={onPress} tooltipLabel={tooltip} />
    );
}
