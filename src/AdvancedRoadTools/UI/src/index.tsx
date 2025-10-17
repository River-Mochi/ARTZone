// File: src/AdvancedRoadTools/UI/src/index.tsx
// Purpose: Wire our React pieces into the game UI registry.
// - Floating button goes to GameTopRight (per your request).
// - Mouse tool options are extended with the Zoning Side section.

import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import { ZoningToolController } from "./mods/ZoningToolSections";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // Was GameTopLeft in the original; you asked for top-right instead.
    moduleRegistry.append("GameTopRight", ZoningToolControllerButton);

    // Extend vanilla mouse tool options with our "Zoning Side" section.
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        ZoningToolController
    );
};

export default register;
