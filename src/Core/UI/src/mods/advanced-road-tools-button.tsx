import { FloatingButton } from "cs2/ui";
import React, { CSSProperties } from 'react';
import menuButtonStyles from "./advanced-road-tools-button.module.scss";

class AdvancedRoadToolsButtonInternal extends React.Component {
	handleMenuButtonClick = () => {
		console.log("ZoningToolkit: Clicked toolkit menu button");
		//useModUIStore.getState().updateUiVisible(!useModUIStore.getState().uiVisible)
	}

	render() {

		return (
				<FloatingButton
					onClick={this.handleMenuButtonClick}
				>
				</FloatingButton>
		);
	}
}

export const AdvancedRoadToolsButton = AdvancedRoadToolsButtonInternal