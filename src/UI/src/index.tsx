// File: src/UI/src/index.tsx
// Purpose: Registrar — wire React bits into vanilla UI.
//   • Floating button at GameTopLeft
//   • “Zoning Side” section in MouseToolOptions
//   • Keep Tool Options panel visible for our tool
//   • Optional: DIAG_TOPO to scan modules in UI.log
//
// Also ensures our images are emitted to coui://ui-mods/images/.

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import ARTZoneToolButton from "./mods/artzone-tool-button";
import { ZoningToolController } from "./mods/ZoningToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure assets are emitted to coui://ui-mods/images/
import "../images/menu-top-icon.svg";            // Top-left FAB icon (matches C# MainIconPath)
import "../images/MapGrid.svg";                  // Road Services panel tile (NEW)

// Mode icons used in the Tool Options section
import "../images/icons/mode-icon-both.svg";
import "../images/icons/mode-icon-left.svg";
import "../images/icons/mode-icon-right.svg";

const DIAG_TOPO = false; // False for Release. Flip to true to scan modules in UI.log.

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

function extendSafe(
    registry: ModuleRegistry,
    modulePath: string,
    exportId: string,
    extension: any
) {
    try {
        registry.extend(modulePath, exportId, extension);
    } catch (err) {
        try {
            console.error(`[ART][UI] extend failed for ${modulePath}#${exportId}`, err);
        } catch {
            /* ignore */
        }
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    if (DIAG_TOPO) {
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
    }

    moduleRegistry.append("GameTopLeft", ARTZoneToolButton);

    extendSafe(
        moduleRegistry,
        VANILLA.MouseToolOptions.path,
        VANILLA.MouseToolOptions.exportId,
        ZoningToolController
    );

    extendSafe(
        moduleRegistry,
        VANILLA.ToolOptionsPanelVisible.path,
        VANILLA.ToolOptionsPanelVisible.exportId,
        ToolOptionsVisibility
    );
};

export default register;
