// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Inject Easy Zoning controls for new roads into the vanilla Tool Options panel.
// Notes:
//   - No custom UI in Photo Mode.
//   - Existing roads are handled by ez-existingRoadsPanel.tsx so EZ does not
//     restyle vanilla Tool Options containers or other mods by accident.

import React from "react";
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";

import mod from "mod.json";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import { rowNewRoads } from "./ez-zoneToolSections.module.scss";

// Icon assets (webpack emits to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";

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
const IsZonableRoadPrefab$ = bindValue<boolean>(mod.id, "IsZonableRoadPrefab");
const IsPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

// Trigger helpers (UI -> C#).
function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
}

function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
}

// Wrap vanilla MouseToolOptions component.
export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        const { translate } = useLocalization();

        // No custom UI in Photo Mode.
        const photoMode = useValue(IsPhotoMode$) === true;

        // Vanilla road tool on a zonable road prefab?
        const roadPrefabActive = useValue(IsZonableRoadPrefab$) === true;

        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

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

        // New roads use the vanilla-like section title.
        const titleZone = translate("EasyZoning.NewRoads.SectionTitle", "Zone Change");

        const tipBoth = translate("EasyZoning.NewRoads.Tooltip.Both", "Both sides.");
        const tipLeft = translate("EasyZoning.NewRoads.Tooltip.Left", "Left side only.");
        const tipRight = translate("EasyZoning.NewRoads.Tooltip.Right", "Right side only.");

        // Only inject into vanilla road tools. Existing-road EZ tool is isolated
        // in its own panel, which avoids touching vanilla/mod-owned CSS wrappers.
        if (!roadPrefabActive) {
            return result;
        }

        const sections: any[] = [];

        // Row 1: zoning icons.
        {
            const selectedMode = roadMode;

            const onLeft = () =>
                setRoadZoningMode(selectedMode === ZoningMode.Left ? ZoningMode.None : ZoningMode.Left);

            const onRight = () =>
                setRoadZoningMode(selectedMode === ZoningMode.Right ? ZoningMode.None : ZoningMode.Right);

            const onBoth = () => flipRoadBothMode();

            sections.push(
                <Section key="EZ_ZoneChange" title={titleZone}>
                    <div className={rowNewRoads}>
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
