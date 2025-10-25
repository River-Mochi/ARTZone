// File: src/UI/src/index.tsx
// Purpose: Registrar — wire React pieces into vanilla UI.
//   • Floating button at GameTopLeft
//   • “Zoning Side” section in MouseToolOptions
//   • Keep Tool Options panel visible for our tool
//   • Ensure main SVG icon is emitted
//
// Design: single source of truth for vanilla paths/ids; tiny guards; strict typing.

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";
import { ZoningToolController } from "./mods/ZoningToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure the icon is emitted to coui://ui-mods/images/ico-4square-color.svg
import "../images/ico-4square-color.svg";

// --- Single source of truth for vanilla modules/exports we touch ---
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

// Guarded extend (don’t crash registrar if a vanilla export shifts in a patch)
function extendSafe(
    registry: ModuleRegistry,
    modulePath: string,
    exportId: string,
    extension: any
) {
    try {
        registry.extend(modulePath, exportId, extension);
    } catch (err) {
        // eslint-disable-next-line no-console
        console.error(`[ART][UI] extend failed for ${modulePath}#${exportId}`, err);
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    // Centralize access to vanilla components for this session
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // --- One-time diagnostic to help locate vanilla Topography / Contours control ---
    // You can see this output in the UI dev tools (http://localhost:9444/) or in Player.log.
    try {
        const candidates = moduleRegistry.find(/topograph|contour|terrain|overlay|elev|height/i);
        if (Array.isArray(candidates) && candidates.length) {
            console.log("[ART][diag] topo candidates:", candidates);
        } else {
            console.log("[ART][diag] no topo/contour candidates found");
        }
    } catch {
        /* silent in production */
    }

    // Put the main toggle button on the top left (vanilla slot)
    moduleRegistry.append("GameTopLeft", ZoningToolControllerButton);

    // Extend the vanilla MouseToolOptions panel with our section (keep us near the bottom)
    extendSafe(
        moduleRegistry,
        VANILLA.MouseToolOptions.path,
        VANILLA.MouseToolOptions.exportId,
        ZoningToolController
    );

    // Force the small Tool Options panel to be visible while our tool is active
    extendSafe(
        moduleRegistry,
        VANILLA.ToolOptionsPanelVisible.path,
        VANILLA.ToolOptionsPanelVisible.exportId,
        ToolOptionsVisibility
    );
};

export default register;
