// File: src/UI/src/index.tsx
// Purpose: Wire the React pieces into the game UI registry.
//  • Appends our floating button at GameTopRight.
//  • Extends the MouseToolOptions panel to render our "Zoning Side" buttons.

import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import { ZoningToolController } from "./mods/ZoningToolSections";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";


// Ensure the palette PNG is emitted to coui://ui-mods/images/ToolsIcon.png
import "../images/Tool_Icon/ToolsIcon.png";


const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // put the main button on the top left
    moduleRegistry.append("GameTopLeft", ZoningToolControllerButton);

    // Adds the "Zoning Side" section with three buttons (Both/Left/Right)
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        ZoningToolController
    );
};

export default register;
