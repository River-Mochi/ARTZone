# CS2 UI Reference (River-Mochi, EasyZoning)
# File: UI-Reference.md

## Button event handlers

### cs2/ui Button
- Prefer `onSelect` for cs2/ui buttons.
  - Intended behavior: fires on mouse click **and** gamepad SELECT.
  - Better alignment with CS2 input/hints/sounds.

- `onClick` exists because cs2/ui Button props extend React ButtonHTMLAttributes.
  - May work for mouse clicks.
  - Not the primary CS2 contract for “selectable” UI actions.

### When to use onClick
- Pointer-centric widgets and components that explicitly document onClick semantics:
  - cs2/input PointerBarrier
  - UI controls that deal in pointer events (e.g., color pickers)

## Icon buttons
- cs2/ui buttons support icon usage via `src`.
- For floating icon buttons, prefer:
  - `<Button variant="floating" src={IconPath} onSelect={...} />`

Tool Buttons, Use onSelect (NOT onClick):
- onSelect is the CS2 UI handler: mouse click OR gamepad SELECT.
- Keep the GTL button independent from keybind conflicts (Ctrl+V can fail, button still works).

### GameTopLeft (GTL) floating button (no scss needed)
- `moduleRegistry.append("GameTopLeft", ...)` inserts the component into a **vanilla-owned UI slot** that already has layout.
- The slot already provides layout (row placement, spacing) and the game’s global button styling.
- Using `cs2/ui` `<Button variant="floating" ... />` uses **built-in CS2 UI styling**, so it matches other mods automatically.
- The icon is passed via `src={...}` (webpack emits the asset to `coui://ui-mods/images/`), so no custom CSS is required just to show it.

**Result:** the GTL launcher button works with **zero SCSS** because the host container + `cs2/ui` button variant already handle visuals.

SCSS is typically only needed for Tool Options panel sections (the panel content area), where layout/spacing and icon rows often need explicit styling.


### Tool Options panel sections (MouseToolOptions) NEEDS SCSS
- Tool Options extensions render inside a larger vanilla panel, but **your section layout** (rows, spacing, alignment, custom icon grids, etc.) is *your responsibility*.
- When adding custom controls/components in panel sections (`...Sections.tsx` / Tool Options UI), SCSS is used to:
  - match vanilla spacing/typography
  - align controls cleanly
  - avoid “unstyled stack of divs”


## Verification
- Local types: `src/UI/types/ui.d.ts` and `src/UI/types/input.d.ts`
- Devtools: http://localhost:9444/
  - Sources → index.js (pretty-print)
  - Search module paths used by ModuleRegistry:
    - game-ui/.../tool-button.tsx
    - game-ui/.../description-tooltip.tsx

