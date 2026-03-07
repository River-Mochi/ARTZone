// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Injects Easy Zoning controls into the Tool Options panel.
//   When relevant, appends EZ sections after vanilla sections so
//   snap/underground rows and other vanilla rows remain visible.
//
// Safety / robustness:
//   - Never inject in PhotoMode (vanilla UI must remain stable).
//   - Never assumes vanilla element shape; returns vanilla unchanged if unexpected.
//   - Never mutates result.props directly (uses React.cloneElement).
//   - All hooks are unconditional calls.

import React from "react";
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

import mod from "mod.json";
import { ZONING_TOOL_ID } from "../shared/tool-ids";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import { row } from "./ez-zoneToolSections.module.scss";

// Icon assets (webpack emits to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";
import IconContour from "../../images/icons/ContourLines.svg";

// NOTE: These numeric values MUST match the C# ZoningMode enum.
// Bit flags: Right=1, Left=2, Both=3.
export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

// Value bindings exported by C# (ZoneControlBridgeUI).
// bindValue() creates a live binding object; useValue() reads it reactively.
const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const IsZonableRoadPrefab$ = bindValue<boolean>(mod.id, "IsZonableRoadPrefab");

// C# binding (ZoneControlBridgeUI exposes IsPhotoMode).
const IsPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

// Trigger helpers (UI -> C#).
function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
}

function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
}

function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
}

function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
}

// registry.extend hook: wraps the vanilla MouseToolOptions component.
// The wrapper renders vanilla first, then conditionally appends EZ sections.
export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        // Hooks must be unconditional and always called in the same order.
        const { translate } = useLocalization();

        // PhotoMode rule: do not inject any custom UI into Tool Options.
        const photoMode = useValue(IsPhotoMode$) === true;

        // Active tool id is a stable identifier for comparisons (vanilla + mods).
        const activeToolId = useValue(tool.activeTool$)?.id;

        // True when a road prefab is selected and supports zoning blocks.
        const roadPrefabActive = useValue(IsZonableRoadPrefab$) === true;

        // True when EZ tool is the active tool.
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        // Current mode state read from bindings.
        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

        // Render vanilla first. If vanilla throws, do not take down the UI.
        let result: any;
        try {
            // Note: in CS2 module registry extensions, Component is often a function.
            result = Component(props);
        } catch (err) {
            try {
                console.error("[EZ][UI] ToolOptions injection: vanilla component threw", err);
            } catch {
                // ignore
            }
            return null;
        }

        // PhotoMode must remain vanilla-clean (no injection).
        if (photoMode) {
            return result;
        }

        // If vanilla returned something unexpected (non-React element), do nothing.
        if (!React.isValidElement(result)) {
            return result;
        }

        // Resolve vanilla UI components (Section, ToolButton, styling theme).
        // Resolver indirection avoids importing private vanilla modules directly.
        let Section: any;
        let ToolButton: any;
        let FOCUS_DISABLED: any;
        let toolButtonClass: any;

        try {
            const resolver = VanillaComponentResolver.instance;
            Section = resolver?.Section;
            ToolButton = resolver?.ToolButton;
            FOCUS_DISABLED = resolver?.FOCUS_DISABLED;
            toolButtonClass = resolver?.toolButtonTheme?.ToolButton ?? undefined;
        } catch {
            return result;
        }

        // If resolver is not ready, keep vanilla unchanged.
        if (typeof Section !== "function" || typeof ToolButton !== "function") {
            return result;
        }

        // Layout class for the row of tool buttons.
        const rowClass = row ?? undefined;

        // Section titles.
        const titleZone = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.SectionTitle]",
            "Zone Change"
        );
        const titleContour = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.ContourTitle]",
            "Contour lines"
        );

        // Tooltips.
        const tipBoth = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeBothDescription]",
            "Toggle zoning on both sides."
        );
        const tipLeft = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeLeftDescription]",
            "Zone left side only."
        );
        const tipRight = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeRightDescription]",
            "Zone right side only."
        );
        const tipContour = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ContourDescription]",
            "Toggle terrain contour lines."
        );

        // Show zone buttons if either:
        // - a zonable road prefab is active (vanilla road tool case), or
        // - EZ tool is active (update-existing case).
        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;

        // When nothing relevant is active, keep vanilla unchanged.
        if (!roadPrefabActive && !zoningToolOn) {
            return result;
        }

        const sections: any[] = [];

        // Zone row: shown for both vanilla road tool (new roads) and EZ tool (update mode).
        if (shouldShowZoneSection) {
            // If a road prefab is active and EZ tool is not, UI edits RoadZoningMode.
            // Otherwise UI edits ToolZoningMode (EZ tool state).
            const usingRoadState = roadPrefabActive && !zoningToolOn;
            const selectedMode = usingRoadState ? roadMode : toolMode;

            const onLeft = () =>
                usingRoadState
                    ? setRoadZoningMode(ZoningMode.Left)
                    : setToolZoningMode(ZoningMode.Left);

            const onRight = () =>
                usingRoadState
                    ? setRoadZoningMode(ZoningMode.Right)
                    : setToolZoningMode(ZoningMode.Right);

            // Both-mode behavior uses “flip” triggers (Both <-> None).
            const onBoth = () =>
                usingRoadState ? flipRoadBothMode() : flipToolBothMode();

            sections.push(
                <Section key="EZ_ZoneChange" title={titleZone}>
                    <div className={rowClass}>
                        <ToolButton
                            selected={(selectedMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={onBoth}
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
                </Section>
            );
        }

        if (sections.length === 0) {
            return result;
        }

        // Merge vanilla children with EZ sections.
        // Appends EZ after vanilla so stock rows remain present.
        const existingChildren = (result as any).props?.children;

        const mergedChildren =
            existingChildren == null
                ? sections
                : Array.isArray(existingChildren)
                    ? [...existingChildren, ...sections]
                    : [existingChildren, ...sections];

        // TS2769 fix: cloneElement typing is strict; cast to any intentionally.
        return React.cloneElement(result as any, undefined, mergedChildren);
    };
};
