// src/UI/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// DO NOT CHANGE the TOOL_ID below.
// It MUST match C# ZoningControllerToolSystem.ToolID ("ARTZone.ZoningTool")
// or the small Tool Options panel won't stay visible for our tool.

import { ModuleRegistryExtend } from "cs2/modding";
import { tool } from "cs2/bindings";

// DO NOT CHANGE – must equal C# ZoningControllerToolSystem.ToolID
const TOOL_ID = "ARTZone.ZoningTool";

export const ToolOptionsVisibility: ModuleRegistryExtend = (useToolOptionsVisible: any) => {
    return (...args: any[]) => {
        const vanillaVisible = useToolOptionsVisible?.(...args);
        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === TOOL_ID;

        // Light breadcrumb (UI.log). Harmless in production.
        try { console.log(`[ART][UI] useToolOptionsVisible: activeId=${activeId} ours=${ours} vanilla=${!!vanillaVisible}`); } catch { }

        return vanillaVisible || ours;
    };
};
