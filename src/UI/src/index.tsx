// File: src/UI/src/index.tsx
// Purpose: Wire the React pieces into the vanilla UI.
//  • Appends our floating button at GameTopLeft.
//  • Extends MouseToolOptions to render the “Zoning Side” buttons.
//  • Ensures ToolsIcon.png is emitted so the palette can reference it.
//  • Forces the Tool Options panel to stay visible for our tool.

import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import { ZoningToolController } from "./mods/ZoningToolSections";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure the palette PNG is emitted to coui://ui-mods/images/ToolsIcon.png
import "../images/Tool_Icon/ToolsIcon.png";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // Put the main toggle button on the top left (vanilla slot)
    moduleRegistry.append("GameTopLeft", ZoningToolControllerButton);

    // Extend the vanilla MouseToolOptions panel with our section (keep us near the bottom)
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        ZoningToolController
    );

    // Force the small Tool Options panel to be visible while our tool is active
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/tool-options-panel.tsx",
        "useToolOptionsVisible",
        ToolOptionsVisibility
    );
};

export default register;
