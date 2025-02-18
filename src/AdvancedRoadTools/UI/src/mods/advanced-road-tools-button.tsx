import { FloatingButton } from "cs2/ui";
import React, { CSSProperties } from 'react';
import menuButtonStyles from "./advanced-road-tools-button.module.scss";
import {trigger} from "cs2/api";
import mod from "../../mod.json";

class AdvancedRoadToolsButtonInternal extends React.Component {
    ToggleZoneControllerTool = () => {
        trigger(mod.id, "ToggleZoneControllerTool");
    }

    render() {

        return (
            <FloatingButton
                onClick={this.ToggleZoneControllerTool}
            >
            </FloatingButton>
        );
    }
}

export const AdvancedRoadToolsButton = AdvancedRoadToolsButtonInternal