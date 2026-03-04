// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Injects Easy Zoning controls into the Tool Options panel.
//   When relevant, appends EZ sections after vanilla sections so
//   snap/underground rows and other vanilla rows remain visible.
//
// Safety / robustness:
//   - Never inject in Photo Mode (vanilla UI must remain stable).
//   - Never assumes vanilla element shape; returns vanilla unchanged if unexpected.
//   - Never mutates result.props directly (uses React.cloneElement).
//   - All hooks are unconditional (no conditional hook calls).

import React from "react";
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

import mod from "mod.json";
import { ZONING_TOOL_ID } from "../shared/tool-ids";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import styles from "./ez-zoneToolSections.module.scss";

// Icon assets (webpack emits to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";
import IconContour from "../../images/icons/ContourLines.svg";

// NOTE: These numeric values must match the C# ZoningMode enum.
export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const IsZonableRoadPrefab$ = bindValue<boolean>(mod.id, "IsZonableRoadPrefab");
const ContourEnabled$ = bindValue<boolean>(mod.id, "ContourEnabled");

// C# binding (ZoningControllerToolUISystem exposes IsPhotoMode)
const IsPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

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

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        // Hooks must be unconditional.
        const { translate } = useLocalization();
        const photoMode = useValue(IsPhotoMode$) === true;

        const activeToolId = useValue(tool.activeTool$)?.id;
        const roadPrefabActive = useValue(IsZonableRoadPrefab$) === true;
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;
        const contourEnabled = !!useValue(ContourEnabled$);

        // Render vanilla first. If vanilla throws, do not take down the UI.
        let result: any;
        try {
            result = Component(props);
        } catch (err) {
            try {
                console.error("[EZ][UI] ToolOptions injection: vanilla component threw", err);
            } catch {
                // ignore
            }
            return null;
        }

        // Photo Mode must remain vanilla-clean (no injection).
        if (photoMode) {
            return result;
        }

        // If vanilla returned something unexpected, do nothing.
        if (!React.isValidElement(result)) {
            return result;
        }

        // Pull vanilla components defensively.
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

        const rowClass = styles.row ?? undefined;

        const titleZone = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.SectionTitle]",
            "Zone Change"
        );
        const titleContour = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.ContourTitle]",
            "Contour"
        );

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

        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;

        // When no road prefab is active and EZ tool is not active, leave vanilla unchanged.
        if (!shouldShowZoneSection && !zoningToolOn) {
            return result;
        }

        const sections: any[] = [];

        // Contour row: only when EZ tool is active (update-existing mode).
        if (zoningToolOn) {
            sections.push(
                <Section key="EZ_Contour" title={titleContour}>
                    <div className={rowClass}>
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

        // Zone row: shown for both vanilla road tool (new roads) and EZ tool (update mode).
        if (shouldShowZoneSection) {
            // When a road prefab is active and EZ tool is not, buttons act on RoadZoningMode.
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
