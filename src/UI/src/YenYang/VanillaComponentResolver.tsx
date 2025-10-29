// File: src/UI/src/YenYang/VanillaComponentResolver.tsx
// Purpose: Resolve vanilla CS2 UI components + themes, with proper React typings.
// Notes:
//   • Import React's JSX/ReactNode types explicitly to satisfy the TS server in VS.
//   • This file only *reads* from the ModuleRegistry; it never hard-crashes if themes are missing.

import type { JSX, HTMLAttributes, ReactNode } from "react";
import { BalloonDirection, Color, FocusKey, Theme, UniqueFocusKey } from "cs2/bindings";
import { InputAction } from "cs2/input";
import type { ModuleRegistry } from "cs2/modding";

// These are specific to the types of components that this mod uses.
// In the UI dev tools at http://localhost:9444/ → Sources → index.js (pretty-print).
// Search for the tsx/scss paths, find the exported functions and their props.
// Types below are intentionally loose to tolerate minor game updates.

type PropsToolButton = {
    focusKey?: UniqueFocusKey | null;
    src?: string;
    selected?: boolean;
    multiSelect?: boolean;
    disabled?: boolean;
    tooltip?: ReactNode | null;
    selectSound?: any;
    uiTag?: string;
    className?: string;
    children?: string | JSX.Element | JSX.Element[];
    onSelect?: (x: any) => any;
} & HTMLAttributes<any>;

type PropsSection = {
    title?: string | null;
    uiTag?: string;
    children: string | JSX.Element | JSX.Element[];
};

type PropsDescriptionTooltip = {
    title: string | null;
    description: string | null;
    content?: JSX.Element | null;
    children?: string | JSX.Element | JSX.Element[];
};

type PropsColorField = {
    focusKey?: FocusKey;
    disabled?: boolean;
    value?: Color; // UnityEngine.Color in C#
    className?: string;
    selectAction?: InputAction;
    alpha?: any;
    popupDirection?: BalloonDirection;
    onChange?: (e: Color) => void;
    onClick?: (e: any) => void;
    onMouseEnter?: (e: any) => void;
    onMouseLeave?: (e: any) => void;
};

// Paths/exports we consume from the game's module registry.
const registryIndex = {
    Section: [
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "Section",
    ],
    ToolButton: ["game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton"],
    toolButtonTheme: [
        "game-ui/game/components/tool-options/tool-button/tool-button.module.scss",
        "classes",
    ],
    mouseToolOptionsTheme: [
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss",
        "classes",
    ],
    FOCUS_DISABLED: ["game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED"],
    FOCUS_AUTO: ["game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"],
    useUniqueFocusKey: ["game-ui/common/focus/focus-key.ts", "useUniqueFocusKey"],
    assetGridTheme: ["game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss", "classes"],
    descriptionTooltipTheme: [
        "game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss",
        "classes",
    ],
    ColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", "ColorField"],
} as const;

export class VanillaComponentResolver {
    public static get instance(): VanillaComponentResolver {
        return this._instance!;
    }
    private static _instance?: VanillaComponentResolver;

    public static setRegistry(in_registry: ModuleRegistry) {
        this._instance = new VanillaComponentResolver(in_registry);
    }

    private registryData: ModuleRegistry;
    private constructor(in_registry: ModuleRegistry) {
        this.registryData = in_registry;
    }

    private cachedData: Partial<Record<keyof typeof registryIndex, any>> = {};

    private updateCache(entry: keyof typeof registryIndex) {
        const [path, exportId] = registryIndex[entry];
        const mod = this.registryData.registry.get(path);
        // Be defensive: if something is missing, cache undefined so we don't throw repeatedly.
        const value = mod ? (mod as any)[exportId] : undefined;
        this.cachedData[entry] = value;
        return value;
    }

    // Component accessors (lazy + cached)
    public get Section(): (props: PropsSection) => JSX.Element {
        return this.cachedData["Section"] ?? this.updateCache("Section");
    }
    public get ToolButton(): (props: PropsToolButton) => JSX.Element {
        return this.cachedData["ToolButton"] ?? this.updateCache("ToolButton");
    }
    public get ColorField(): (props: PropsColorField) => JSX.Element {
        return this.cachedData["ColorField"] ?? this.updateCache("ColorField");
    }

    // Theme accessors (can be undefined on some installs → caller should tolerate it)
    public get toolButtonTheme(): Theme | any {
        return this.cachedData["toolButtonTheme"] ?? this.updateCache("toolButtonTheme");
    }
    public get mouseToolOptionsTheme(): Theme | any {
        return this.cachedData["mouseToolOptionsTheme"] ?? this.updateCache("mouseToolOptionsTheme");
    }
    public get assetGridTheme(): Theme | any {
        return this.cachedData["assetGridTheme"] ?? this.updateCache("assetGridTheme");
    }
    public get descriptionTooltipTheme(): Theme | any {
        return (
            this.cachedData["descriptionTooltipTheme"] ?? this.updateCache("descriptionTooltipTheme")
        );
    }

    // Focus helpers
    public get FOCUS_DISABLED(): UniqueFocusKey {
        return this.cachedData["FOCUS_DISABLED"] ?? this.updateCache("FOCUS_DISABLED");
    }
    public get FOCUS_AUTO(): UniqueFocusKey {
        return this.cachedData["FOCUS_AUTO"] ?? this.updateCache("FOCUS_AUTO");
    }
    public get useUniqueFocusKey(): (focusKey: FocusKey, debugName: string) => UniqueFocusKey | null {
        return this.cachedData["useUniqueFocusKey"] ?? this.updateCache("useUniqueFocusKey");
    }
}
