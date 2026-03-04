// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep the Tool Options panel visible while the Easy Zoning tool is active.

import { tool } from "cs2/bindings";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

type UseToolOptionsVisible = (...args: any[]) => boolean;
type ExtendHook<T extends (...args: any[]) => any> = (original: T) => T;

export const ToolOptionsVisibility: ExtendHook<UseToolOptionsVisible> = (useToolOptionsVisible) => {
    return (...args: any[]) => {
        const vanillaVisible = !!useToolOptionsVisible?.(...args);

        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;

        return vanillaVisible || ours;
    };
};
