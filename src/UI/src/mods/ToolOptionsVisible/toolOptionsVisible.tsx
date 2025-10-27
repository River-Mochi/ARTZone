// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose: Keep the small Tool Options panel visible while our tool is active.
// NOTE: Compares against shared ZONING_TOOL_ID only.

import { ModuleRegistryExtend } from "cs2/modding";
import { tool } from "cs2/bindings";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

export const ToolOptionsVisibility: ModuleRegistryExtend = (useToolOptionsVisible: any) => {
    return (...args: any[]) => {
        const vanillaVisible = useToolOptionsVisible?.(...args);
        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;

        try { console.log(`[ART][UI] useToolOptionsVisible: activeId=${activeId} ours=${ours} vanilla=${!!vanillaVisible}`); } catch { }

        return vanillaVisible || ours;
    };
};
