// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose: Force the small Tool Options panel to stay visible while our tool is active.
// Note: This must be extended into "game-ui/game/components/tool-options/tool-options-panel.tsx"
//       export "useToolOptionsVisible" (exactly as you did in index.tsx).

import { ModuleRegistryExtend } from "cs2/modding";
import { tool } from "cs2/bindings";

// Our tool ID as used by C# (ZoningControllerToolSystem.toolID)
const TOOL_ID = "Zone Controller Tool";

export const ToolOptionsVisibility: ModuleRegistryExtend = (useToolOptionsVisible: any) => {
    // Vanilla exports use a hook-like function; we need to return a function with the same signature.
    return (...args: any[]) => {
        // What vanilla would decide:
        const vanillaVisible = useToolOptionsVisible?.(...args);

        // Keep panel visible for our tool as well.
        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === TOOL_ID;

        // If vanilla already wants it, keep it; otherwise force true for our tool.
        return vanillaVisible || ours;
    };
};
