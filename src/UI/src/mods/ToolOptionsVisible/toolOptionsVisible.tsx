// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep Tool Options panel visible while the EZ tool is active.
// Notes to future me:
//   - useValue(...) subscribes to activeTool$ so visibility updates immediately on tool changes.
//   - Do NOT use optional chaining when calling useToolOptionsVisible.
//     If vanilla export breaks on patch day, better to fail loudly.
//   - Do a one-time "is it a function" guard at extension time (not per-render) to avoid hook - order weirdness.

import { trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import mod from "mod.json";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

type UseToolOptionsVisible = (...args: any[]) => boolean;
type ExtendHook<T extends (...args: any[]) => any> = (original: T) => T;

// Extends vanilla useToolOptionsVisible().
// If vanilla wants Tool Options visible OR EZ tool is active, keep the panel open.
export const ToolOptionsVisibility: ExtendHook<UseToolOptionsVisible> = (useToolOptionsVisible) => {
    // One-time validation when the hook is registered.
    // Runs once at mod UI init time, not every render.
    const vanillaFnOk = typeof useToolOptionsVisible === "function";

    // Send errors to both console and bridge to log file
    if (!vanillaFnOk) {
        console.error("[EZ][UI] useToolOptionsVisible missing or not a function; falling back to EZ-only visibility.");
        try {
            trigger(mod.id, "UILogWarn", "useToolOptionsVisible missing; falling back to EZ-only Tool Options visibility.");
        } catch {
        }
    }

    return (...args: any[]) => {
        // Preserve vanilla behavior first when available.
        // No optional chaining here: in normal operation this MUST exist.
        const vanillaVisible = vanillaFnOk ? !!useToolOptionsVisible(...args) : false;

        // Reactive read: re-render when active tool changes (id is stable identifier)
        const activeId = useValue(tool.activeTool$)?.id;
        const ours = activeId === ZONING_TOOL_ID;

        // Keep Tool Options open when EZ tool is active (vanilla may hide it otherwise).
        return vanillaVisible || ours;
    };
};
