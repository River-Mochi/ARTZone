// File: src/UI/src/index.tsx
// Purpose: Registrar — wire React bits into vanilla UI.
//   • Floating button at GameTopLeft
//   • “Zoning Side” section in MouseToolOptions
//   • Keep Tool Options panel visible for our tool
//   • Ensure main SVG icon is emitted
//   • Log Topography/Contour candidates to UI.log for follow-up

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import ARTZoneToolButton from "./mods/artzone-tool-button";
import { ZoningToolController } from "./mods/ZoningToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure the icon is emitted to coui://ui-mods/images/ico-4square-color.svg
import "../images/ico-4square-color.svg";

// Single source of truth for vanilla modules/exports we touch
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
        // Goes to UI.log in dev mode; safe to keep in production too.
        console.error(`[ART][UI] extend failed for ${modulePath}#${exportId}`, err);
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    // Centralize access to vanilla components for this session
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // One-time diagnostic to help locate vanilla Topography / Contours control.
    // You’ll see this in UI.log (short and easy to read).
    try {
        const candidates = moduleRegistry.find(/topograph|contour|terrain|overlay|elev|height/i);
        if (Array.isArray(candidates) && candidates.length) {
            for (const [path, ...exports] of candidates ?? []) {
                if (/tool-options|topograph|contour/i.test(path)) {
                    console.log(`[ART][diag] candidate: ${path}  ->  ${exports.join(",")}`);
                }
            }

        } else {
            console.log("[ART][diag] no topo/contour candidates found");
        }
    } catch {
        /* silent */
    }

    // Put the main toggle button on the top left (vanilla slot)
    moduleRegistry.append("GameTopLeft", ARTZoneToolButton);

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
