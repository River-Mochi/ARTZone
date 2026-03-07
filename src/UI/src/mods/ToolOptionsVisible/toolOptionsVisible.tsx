// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep the Tool Options panel visible while the Easy Zoning tool is active.
// Notes:
//   - useValue(...) subscribes to activeTool$ so visibility updates immediately on tool changes.

import { useValue } from "cs2/api";
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

        // Reactive read: re-render when active tool changes (id is stable identifier)
        const activeId = useValue(tool.activeTool$)?.id;
        const ours = activeId === ZONING_TOOL_ID;

        // Keep Tool Options open when EZ tool is active (vanilla may hide it otherwise).
        return vanillaVisible || ours;
    };
};
