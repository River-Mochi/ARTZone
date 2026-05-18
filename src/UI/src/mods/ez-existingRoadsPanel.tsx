// File: src/UI/src/mods/ez-existingRoadsPanel.tsx
// Purpose:
//   Builds the EZ-owned compact panel and buttons for updating existing roads.
// Notes:
//   - buttons: Both, Left, Right, Contour
//   - Appended to Game, not injected into vanilla Tool Options.
//   - Keep styling in ez-existingRoadsPanel.module.scss only.
//   - Do not use wildcard selectors against vanilla / mod - owned wrapper classes.

import React from "react";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

import mod from "mod.json";
import { ZONING_TOOL_ID } from "../shared/tool-ids";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import { contourRow, panel, panelGlass, panelVanilla, row } from "./ez-existingRoadsPanel.module.scss";

// Icon assets (webpack emits to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";
import IconContour from "../../images/icons/ContourLines.svg";

// NOTE: These numeric values MUST match the C# ZoningMode enum.
// Bit flags: Right=1, Left=2, Both=3.
enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

// Value bindings exported by C# (ZoneControlBridgeUI).
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const ContourEnabled$ = bindValue<boolean>(mod.id, "ContourEnabled");
const ShowContourButton$ = bindValue<boolean>(mod.id, "ShowContourButton");
const UseGlassPanel$ = bindValue<boolean>(mod.id, "UseGlassPanel");
const IsPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

// Trigger helpers (UI -> C#).
function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
}

function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
}

function toggleContourLines() {
    trigger(mod.id, "ToggleContourLines");
}

export default function ExistingRoadsPanel() {
    const { translate } = useLocalization();

    const activeToolId = useValue(tool.activeTool$)?.id;
    const zoningToolOn = activeToolId === ZONING_TOOL_ID;
    const photoMode = useValue(IsPhotoMode$) === true;
    const selectedMode = useValue(ToolZoningMode$) as ZoningMode;
    const contourEnabled = !!useValue(ContourEnabled$);
    const showContourButton = useValue(ShowContourButton$) !== false;
    const useGlassPanel = useValue(UseGlassPanel$) !== false;

    if (!zoningToolOn || photoMode) {
        return null;
    }

    let ToolButton: any;
    let FOCUS_DISABLED: any;
    let toolButtonClass: any;

    try {
        const resolver = VanillaComponentResolver.instance;
        ToolButton = resolver?.ToolButton;
        FOCUS_DISABLED = resolver?.FOCUS_DISABLED;
        toolButtonClass = resolver?.toolButtonTheme?.ToolButton ?? undefined;
    } catch {
        return null;
    }

    if (typeof ToolButton !== "function") {
        return null;
    }

    const tipBoth = translate("EasyZoning.ExistingRoads.Tooltip.Both", "Both sides. Click twice for None.");
    const tipLeft = translate("EasyZoning.ExistingRoads.Tooltip.Left", "Left side only. Right-click cycles zones.");
    const tipRight = translate("EasyZoning.ExistingRoads.Tooltip.Right", "Right side only. Right-click cycles zones.");
    const tipContour = translate("EasyZoning.ExistingRoads.Tooltip.Contour", "Show terrain contour lines.");

    const onLeft = () =>
        setToolZoningMode(selectedMode === ZoningMode.Left ? ZoningMode.None : ZoningMode.Left);

    const onRight = () =>
        setToolZoningMode(selectedMode === ZoningMode.Right ? ZoningMode.None : ZoningMode.Right);

    const panelClass = `${panel} ${useGlassPanel ? panelGlass : panelVanilla}`;

    return (
        <div className={panelClass}>
            <div className={row}>
                <ToolButton
                    selected={(selectedMode & ZoningMode.Both) === ZoningMode.Both}
                    tooltip={tipBoth}
                    onSelect={flipToolBothMode}
                    src={IconBoth}
                    focusKey={FOCUS_DISABLED}
                    className={toolButtonClass}
                />
                <ToolButton
                    selected={(selectedMode & ZoningMode.Left) === ZoningMode.Left}
                    tooltip={tipLeft}
                    onSelect={onLeft}
                    src={IconLeft}
                    focusKey={FOCUS_DISABLED}
                    className={toolButtonClass}
                />
                <ToolButton
                    selected={(selectedMode & ZoningMode.Right) === ZoningMode.Right}
                    tooltip={tipRight}
                    onSelect={onRight}
                    src={IconRight}
                    focusKey={FOCUS_DISABLED}
                    className={toolButtonClass}
                />
            </div>

            {showContourButton && (
                <div className={`${row} ${contourRow}`}>
                    <ToolButton
                        selected={contourEnabled}
                        tooltip={tipContour}
                        onSelect={toggleContourLines}
                        src={IconContour}
                        focusKey={FOCUS_DISABLED}
                        className={toolButtonClass}
                    />
                </div>
            )}
        </div>
    );
}
