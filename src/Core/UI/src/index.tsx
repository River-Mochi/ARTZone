import { ModRegistrar } from "cs2/modding";
import { AdvancedRoadToolsButton } from "mods/advanced-road-tools-button";
import { VanillaComponentResolver} from "./YenYang/VanillaComponentResolver";
import {ZoningToolController} from "./mods/ZoningToolSections";
import React from "react";

const register: ModRegistrar = (moduleRegistry) => {
    // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
    VanillaComponentResolver.setRegistry(moduleRegistry);
    
    
    moduleRegistry.append('GameTopLeft', () => <AdvancedRoadToolsButton/>);

    // This extends mouse tool options to include all tree controller sections.
    moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', ZoningToolController);
}

export default register;