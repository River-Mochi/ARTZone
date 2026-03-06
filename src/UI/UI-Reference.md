# CS2 UI Reference (River-Mochi, EasyZoning)

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

## Verification
- Local types: `src/UI/types/ui.d.ts` and `src/UI/types/input.d.ts`
- Devtools: http://localhost:9444/
  - Sources → index.js (pretty-print)
  - Search module paths used by ModuleRegistry:
    - game-ui/.../tool-button.tsx
    - game-ui/.../description-tooltip.tsx
