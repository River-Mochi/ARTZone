// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Inject Easy Zoning controls into the vanilla Tool Options panel.
//   Existing-roads tool uses a compact transparent layout.
// Notes:
//   - No custom UI in Photo Mode.
//   - New-road vanilla tool panel keeps vanilla labels.
//   - Existing-roads layout uses 2 rows:
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

import { row, rowExisting } from "./ez-zoneToolSections.module.scss";

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

        // Never inject custom UI into Photo Mode.
        const photoMode = useValue(IsPhotoMode$) === true;

        // EZ existing-roads tool active?
        const activeToolId = useValue(tool.activeTool$)?.id;
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        // Vanilla road tool on a zonable road prefab?
        const roadPrefabActive = useValue(IsZonableRoadPrefab$) === true;

        // Apply compact transparent styling only while EZ existing-roads tool is active.
        // Delay on removal helps reduce the brief vanilla close flash.
        React.useEffect(() => {
            const cls = "ez-tooloptions-glass";
            const removeDelayMs = 220;
            let removeTimer: number | undefined;

            try {
                if (zoningToolOn && !photoMode) {
                    document.body.classList.add(cls);
                } else {
                    removeTimer = window.setTimeout(() => {
                        try {
                            document.body.classList.remove(cls);
                        } catch {
                        }
                    }, removeDelayMs);
                }
            } catch {
            }

            return () => {
                if (removeTimer !== undefined) {
                    window.clearTimeout(removeTimer);
                }

                try {
                    if (zoningToolOn && !photoMode) {
                        document.body.classList.remove(cls);
                    }
                } catch {
                }
            };
        }, [zoningToolOn, photoMode]);

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

        // New roads keep the normal row class.
        // Existing roads use the compact right-aligned row class.
        const activeRowClass = zoningToolOn ? rowExisting : row;

        // Section labels:
        // - keep vanilla-like labels for new roads
        // - hide labels for EZ existing-roads compact panel
        const titleZone = zoningToolOn
            ? null
            : translate(
                "ToolOptions.SECTION[EZ.Zone_Controller.SectionTitle]",
                "Zone Change"
            );

        const titleContour = zoningToolOn
            ? null
            : translate(
                "ToolOptions.SECTION[EZ.Zone_Controller.ContourTitle]",
                "Contour"
            );

        // Tooltip keys from /lang/en-US.json.
        // Fallback text is intentionally shorter so missing localization is obvious.
        const tipBoth = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EZ.Zone_Controller.ZoningModeBothDescription]",
            "Both sides."
        );
        const tipLeft = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EZ.Zone_Controller.ZoningModeLeftDescription]",
            "Left only."
        );
        const tipRight = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EZ.Zone_Controller.ZoningModeRightDescription]",
            "Right only."
        );
        const tipContour = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EZ.Zone_Controller.ContourDescription]",
            "Contour lines."
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
                    ? setRoadZoningMode(ZoningMode.Left)
                    : setToolZoningMode(ZoningMode.Left);

            const onRight = () =>
                usingRoadState
                    ? setRoadZoningMode(ZoningMode.Right)
                    : setToolZoningMode(ZoningMode.Right);

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

        // Row 2: contour icon, only for EZ existing-roads tool.
        if (zoningToolOn) {
            sections.push(
                <Section key="EZ_Contour" title={titleContour}>
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
