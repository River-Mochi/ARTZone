// File: src/UI/src/index.tsx
// Purpose: Registrar — wire React pieces into vanilla UI.
//   • Floating button at GameTopLeft
//   • “Zoning Side” section in MouseToolOptions
//   • Keep Tool Options panel visible for our tool
//   • Ensure main SVG icon is emitted
//
// Design: single source of truth for vanilla paths/ids; tiny guards; strict typing
// to select the correct `append(AppendHookTargets, ...)` overload with your d.ts.

import type { ModRegistrar, ModuleRegistry, AppendHookTargets } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";
import { ZoningToolController } from "./mods/ZoningToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Emit the icon into coui://ui-mods/images/...
import "../images/ico-4square-color.svg";

// --- Single source of truth for vanilla modules/exports ---
const APPEND_TARGET: AppendHookTargets = "GameTopLeft"; // typed literal => picks the right overload

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
    // Allow our resolver to fetch vanilla components & themes
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // 1) Floating top-left button (typed union => append(target, component) overload)
    moduleRegistry.append(APPEND_TARGET, ZoningToolControllerButton);

    // 2) Inject our “Zoning Side” section
    extendSafe(
        moduleRegistry,
        VANILLA.MouseToolOptions.path,
        VANILLA.MouseToolOptions.exportId,
        ZoningToolController
    );

    // 3) Keep Tool Options panel visible while our tool is active
    extendSafe(
        moduleRegistry,
        VANILLA.ToolOptionsPanelVisible.path,
        VANILLA.ToolOptionsPanelVisible.exportId,
        ToolOptionsVisibility
    );
};

export default register;
