import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver} from "./YenYang/VanillaComponentResolver";
import {ZoningToolController} from "./mods/ZoningToolSections";
import ZoningToolControllerButton from "./mods/advanced-road-tools-button";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    
    moduleRegistry.append('GameTopLeft', ZoningToolControllerButton);
    
    // This extends mouse tool options to include all tree controller sections.
    moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', ZoningToolController);
}

export default register;