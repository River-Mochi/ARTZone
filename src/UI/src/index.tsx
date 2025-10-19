// File: src/UI/src/index.tsx
// Purpose: Wire the React pieces into the game UI registry.
//  • Appends our floating button at GameTopRight.
//  • Extends the MouseToolOptions panel to render our "Zoning Side" buttons.

import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import { ZoningToolController } from "./mods/ZoningToolSections";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // Your request: put the button on the top right instead of top left.
    moduleRegistry.append("GameTopRight", ZoningToolControllerButton);

    // Adds the "Zoning Side" section with three buttons (All/Left/Right).
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        ZoningToolController
    );
};

export default register;
