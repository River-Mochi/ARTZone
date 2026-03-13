// File: src/UI/src/index.tsx
// Purpose: Hook the UI into vanilla, register top-left button + Tool Options
// section. Keep the options panel visible when the zone tool is active.
// UI dev mode: launch with --uiDeveloperMode and open localhost:9444.

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./components/VanillaComponentResolver";
import mod from "mod.json";
import "./mods/toolOptionsGlass.scss";

import EasyZoningToolButton from "./mods/ez-zone-tool-button";
import { ZoningToolController } from "./mods/ez-zoneToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure assets are emitted to coui://ui-mods/images/
import "../images/ico-zones-color02.svg"; // Top-left FAB icon

// Mode icons used in the Tool panel section
import "../images/icons/mode-icon-both.svg";
import "../images/icons/mode-icon-left.svg";
import "../images/icons/mode-icon-right.svg";
import "../images/icons/ContourLines.svg";

// Vanilla targets being extended/overridden.
// If a game patch changes these paths/exports, extendSafe() prevents total UI failure.
const VANILLA = {
    MouseToolOptions: {
        path: "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        exportId: "MouseToolOptions",
    },
    ToolOptionsPanelVisible: {
        path: "game-ui/game/components/tool-options/tool-options-panel.tsx",
        exportId: "useToolOptionsVisible",
    },
};

// Wrap registry.extend to keep UI alive when a single hook fails after a game patch.
function extendSafe(
    registry: ModuleRegistry,
    modulePath: string,
    exportId: string,
    extension: any
) {
    try {
        registry.extend(modulePath, exportId, extension);
    } catch (err) {
        console.error(`[EZ][UI] extend failed for ${modulePath}#${exportId}`, err);
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    // Store registry for VanillaComponentResolver usage.
    VanillaComponentResolver.setRegistry(moduleRegistry);

    console.log(mod.id + " UI module registrations started.");

    // Add floating button to GameTopLeft region.
    moduleRegistry.append("GameTopLeft", EasyZoningToolButton);

    // Extend Tool Options section to include EZ UI controls.
    extendSafe(
        moduleRegistry,
        VANILLA.MouseToolOptions.path,
        VANILLA.MouseToolOptions.exportId,
        ZoningToolController
    );

    // Keep Tool panel visible when EZ tool is active.
    extendSafe(
        moduleRegistry,
        VANILLA.ToolOptionsPanelVisible.path,
        VANILLA.ToolOptionsPanelVisible.exportId,
        ToolOptionsVisibility
    );

    console.log(mod.id + " UI module registration completed.");
};

export default register;
