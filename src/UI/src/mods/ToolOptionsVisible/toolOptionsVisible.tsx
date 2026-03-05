// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep the Tool Options panel visible while the Easy Zoning tool is active.

import { tool } from "cs2/bindings";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

type UseToolOptionsVisible = (...args: any[]) => boolean;
type ExtendHook<T extends (...args: any[]) => any> = (original: T) => T;

// Extends vanilla useToolOptionsVisible().
// If vanilla wants Tool Options visible OR EZ tool is active, keep the panel open.
export const ToolOptionsVisibility: ExtendHook<UseToolOptionsVisible> = (useToolOptionsVisible) => {
    return (...args: any[]) => {
        // Preserve vanilla behavior first.
        const vanillaVisible = !!useToolOptionsVisible?.(...args);

        // Coherent binding: current active tool instance (id is stable identifier).
        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;

        // Force-visible when EZ tool is active so buttons remain accessible.
        return vanillaVisible || ours;
    };
};
