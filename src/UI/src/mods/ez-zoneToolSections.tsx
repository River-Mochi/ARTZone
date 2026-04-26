// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Inject Easy Zoning controls into the vanilla Tool Options panel.
// Notes:
//   - No custom UI in Photo Mode.
//   - New roads keep vanilla labels and use new-road tooltip text.
//   - Existing roads use a compact panel with 2 rows:
//       row 1 = zoning icons
//       row 2 = contour icon

import React from "react";
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

import mod from "mod.json";
import { ZONING_TOOL_ID } from "../shared/tool-ids";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import { rowExisting, rowNewRoads } from "./ez-zoneToolSections.module.scss";

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
const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const IsZonableRoadPrefab$ = bindValue<boolean>(mod.id, "IsZonableRoadPrefab");
const ContourEnabled$ = bindValue<boolean>(mod.id, "ContourEnabled");
const ShowContourButton$ = bindValue<boolean>(mod.id, "ShowContourButton");
const UseGlassPanel$ = bindValue<boolean>(mod.id, "UseGlassPanel");
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

function toggleContourLines() {
    trigger(mod.id, "ToggleContourLines");
}

// Wrap vanilla MouseToolOptions component.
export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        const { translate } = useLocalization();

        // No custom UI in Photo Mode.
        const photoMode = useValue(IsPhotoMode$) === true;

        // EZ existing-roads tool active?
        const activeToolId = useValue(tool.activeTool$)?.id;
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        // Vanilla road tool on a zonable road prefab?
        const roadPrefabActive = useValue(IsZonableRoadPrefab$) === true;

        // Options-driven UI toggles.
        const showContourButton = useValue(ShowContourButton$) !== false;
        const useGlassPanel = useValue(UseGlassPanel$) !== false;

        // Apply compact existing-roads classes only while EZ tool is active.
        // Small removal delay helps reduce the brief close flash.
        React.useEffect(() => {
            const layoutCls = "ez-tooloptions-existing";
            const glassCls = "ez-tooloptions-glass";
            const vanillaCls = "ez-tooloptions-vanilla";
            const removeDelayMs = 220;
            let removeTimer: number | undefined;

            const clearClasses = () => {
                try {
                    document.body.classList.remove(layoutCls);
                    document.body.classList.remove(glassCls);
                    document.body.classList.remove(vanillaCls);
                } catch {
                }
            };

            try {
                if (zoningToolOn && !photoMode) {
                    document.body.classList.add(layoutCls);
                    document.body.classList.toggle(glassCls, useGlassPanel);
                    document.body.classList.toggle(vanillaCls, !useGlassPanel);
                } else {
                    removeTimer = window.setTimeout(() => {
                        clearClasses();
                    }, removeDelayMs);
                }
            } catch {
            }

            return () => {
                if (removeTimer !== undefined) {
                    window.clearTimeout(removeTimer);
                }

                // Immediate cleanup while the active EZ state still owns the class.
                if (zoningToolOn && !photoMode) {
                    clearClasses();
                }
            };
        }, [zoningToolOn, photoMode, useGlassPanel]);

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;
        const contourEnabled = !!useValue(ContourEnabled$);

        // Render vanilla first.
        let result: any;
        try {
            result = Component(props);
        } catch (err) {
            try {
                console.error("[EZ][UI] ToolOptions injection: vanilla component threw", err);
            } catch {
            }
            return null;
        }

        if (photoMode) {
            return result;
        }

        if (!React.isValidElement(result)) {
            return result;
        }

        // Resolve vanilla pieces through registry wrapper.
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

        if (typeof Section !== "function" || typeof ToolButton !== "function") {
            return result;
        }

        // Existing roads use compact row layout.
        // New roads keep the normal row layout.
        const activeRowClass = zoningToolOn ? rowExisting : rowNewRoads;

        // New roads use the vanilla-like section title.
        // Existing roads hide the labels and use icon-only compact layout.
        const titleZone = zoningToolOn
            ? null
            : translate("EasyZoning.NewRoads.SectionTitle", "Zone Change");

        // Decide which tooltip set applies.
        const usingNewRoadsState = roadPrefabActive && !zoningToolOn;

        const tipBoth = usingNewRoadsState
            ? translate("EasyZoning.NewRoads.Tooltip.Both", "Both sides.")
            : translate("EasyZoning.ExistingRoads.Tooltip.Both", "Both sides. Click twice for None.");

        const tipLeft = usingNewRoadsState
            ? translate("EasyZoning.NewRoads.Tooltip.Left", "Left side only.")
            : translate("EasyZoning.ExistingRoads.Tooltip.Left", "Left side only. Right-click cycles zones.");

        const tipRight = usingNewRoadsState
            ? translate("EasyZoning.NewRoads.Tooltip.Right", "Right side only.")
            : translate("EasyZoning.ExistingRoads.Tooltip.Right", "Right side only. Right-click cycles zones.");

        const tipContour = translate(
            "EasyZoning.ExistingRoads.Tooltip.Contour",
            "Show terrain contour lines."
        );

        // Show EZ controls for:
        // - vanilla road tool on a zonable road prefab
        // - EZ existing-roads tool
        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;
        if (!shouldShowZoneSection) {
            return result;
        }

        const sections: any[] = [];

        // Row 1: zoning icons.
        {
            const usingRoadState = roadPrefabActive && !zoningToolOn;
            const selectedMode = usingRoadState ? roadMode : toolMode;

            const onLeft = () =>
                usingRoadState
                    ? setRoadZoningMode(selectedMode === ZoningMode.Left ? ZoningMode.None : ZoningMode.Left)
                    : setToolZoningMode(selectedMode === ZoningMode.Left ? ZoningMode.None : ZoningMode.Left);

            const onRight = () =>
                usingRoadState
                    ? setRoadZoningMode(selectedMode === ZoningMode.Right ? ZoningMode.None : ZoningMode.Right)
                    : setToolZoningMode(selectedMode === ZoningMode.Right ? ZoningMode.None : ZoningMode.Right);

            const onBoth = () =>
                usingRoadState
                    ? flipRoadBothMode()
                    : flipToolBothMode();

            sections.push(
                <Section key="EZ_ZoneChange" title={titleZone}>
                    <div className={activeRowClass}>
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

        // Row 2: contour icon, existing roads only, and only when enabled in Options.
        if (zoningToolOn && showContourButton) {
            sections.push(
                <Section key="EZ_Contour" title={null}>
                    <div className={activeRowClass}>
                        <ToolButton
                            selected={contourEnabled}
                            tooltip={tipContour}
                            onSelect={toggleContourLines}
                            src={IconContour}
                            focusKey={FOCUS_DISABLED}
                            className={toolButtonClass}
                        />
                    </div>
                </Section>
            );
        }

        const existingChildren = (result as any).props?.children;

        const mergedChildren =
            existingChildren == null
                ? sections
                : Array.isArray(existingChildren)
                    ? [...existingChildren, ...sections]
                    : [existingChildren, ...sections];

        return React.cloneElement(result as any, undefined, mergedChildren);
    };
};
