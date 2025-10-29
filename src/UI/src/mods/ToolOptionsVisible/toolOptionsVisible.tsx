// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose: Keep the small Tool Options panel visible while our tool OR a road prefab is active.

import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue } from "cs2/api";
import { tool } from "cs2/bindings";
import mod from "mod.json";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

// Bound from C#: ZoningControllerToolUISystem
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");

export const ToolOptionsVisibility: ModuleRegistryExtend = (useToolOptionsVisible: any) => {
    return (...args: any[]) => {
        const vanillaVisible = useToolOptionsVisible?.(...args);
        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;
        const roadPrefab = !!isRoadPrefab$.value;

        try {
            console.log(`[EZ][UI] useToolOptionsVisible: active=${activeId} ours=${ours} roadPrefab=${roadPrefab} vanilla=${!!vanillaVisible}`);
        } catch { }

        // Keep the panel visible if vanilla says so, or our tool is active, or a road prefab tool is active.
        return vanillaVisible || ours || roadPrefab;
    };
};
