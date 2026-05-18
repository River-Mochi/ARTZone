# Easy Zoning Internals

Developer notes for future maintenance. This file is intentionally more technical than the README.

## UI Layout

Easy Zoning has two UI paths:

- New roads: EZ extends the vanilla road `MouseToolOptions` panel and adds the zoning side buttons there.
- Existing roads: EZ renders its own compact panel with `moduleRegistry.append("Game", ExistingRoadsPanel)`.

The existing-road panel used to restyle vanilla Tool Options. That caused compatibility problems because wildcard CSS selectors could catch other mods' class names. The current panel is EZ-owned UI and uses only `ez-existingRoadsPanel.module.scss`.

Important rule: do not target vanilla or other-mod class fragments such as `wrapper_`, `item_`, or `content_`. CSS module suffixes protect normal class names, but wildcard selectors bypass that protection.

## Localization

There are two localization systems in this project:

- `src/Localization/Locale*.cs`: Options UI and settings text.
- `src/lang/*.json`: in-city React UI tooltips and section text used through `translate(...)`.

Both sets need to stay key-aligned with English. Run:

```powershell
python src\Scripts\check_all_locales.py
```

## Game Patch 1.5.6f1 Road Zoning

CS2 1.5.6f1 added a vanilla zoning-side tool for existing roads. The important vanilla state is now stored on road composition side flags:

- `Game.Prefabs/CompositionFlags.cs`
- `CompositionFlags.Side.ZonesDisabled`
- `Game.Net/Upgraded.m_Flags.m_Left`
- `Game.Net/Upgraded.m_Flags.m_Right`

EZ should read and write those vanilla flags where possible so players can swap between EZ and the vanilla FAB without desync.

Useful vanilla files from the decompiled game:

- `Game.Prefabs/NetCompositionHelpers.cs`: writes `ZonesDisabled`.
- `Game.Prefabs/NetInitializeSystem.cs`: zoneable roads include `ZonesDisabled` in the side flag mask.
- `Game.Tools/NetToolSystem.cs`: treats `ZonesDisabled` like other side road-upgrade flags.
- `Game.Net/CompositionSelectSystem.cs`: carries flags through upgrade/replace flows.
- `Game.Zones/ZoneToggleJob.cs`: applies side zoning disable behavior.

## Preview Behavior

Vanilla removal preview is not just a colored mesh. It comes through highlighted zone cells:

- `Game.Tools/ZoneGridHighlighted.cs`
- `Game.Tools/GenerateEdgesSystem.cs`
- `Game.Zones/CellCheckSystem.cs`
- `Game.Zones/ZoneCellHighlightJob.cs`
- `Game.Zones/CellFlags.Highlight`
- `Game.Zones/ZoneUtils.GetColorIndex(...)`

EZ uses the vanilla highlight path for removal previews, then `PreviewColorOverrideSystem` adjusts the shader color arrays for readability.

Important fields and shader names:

- `Game.Prefabs/ZoneSystem.m_EdgeColorArray`
- `Game.Prefabs/ZoneSystem.m_FillColorArray`
- `colossal_ZoneEdgeColors`
- `colossal_ZoneFillColors`

Default/reset preview style is high contrast:

- border: orange
- border opacity: 100%
- fill: none
- fill opacity: 100% but ignored while fill is none

Existing players with saved `.coc` settings keep their saved values. Defaults only apply to new installs, reset settings, or missing setting keys.

The fallback in `PreviewColorOverrideSystem` is only for frames where `Mod.Settings` is not available. It should match the default/reset preset so there is no one-frame mismatch.

## New Roads

New roads use the vanilla road tool. EZ stores the selected side mode from the new-road UI and applies it to freshly created road temp entities.

Important system:

- `SyncNewRoadsSystem`

Important guard:

- Only touch true freshly drawn roads where `Temp.m_Original == Entity.Null`.

Vanilla existing-road FAB previews also use temp/create entities, but they mirror an original road. If EZ changes those, the vanilla tool gets stuck to the last EZ side mode.

## Existing Roads

Existing roads use the EZ tool and the existing-road panel. Preview should not commit live zoning changes until the player applies with left click.

Protection toggles must run before writing `ZonesDisabled`:

- Prevent buildings from being removed: if protected occupied cells exist on the side, keep that whole side enabled.
- Do not reset existing zoned squares: if protected painted zoning exists on the side, keep that whole side enabled.

Whole-side protection is required because `ZonesDisabled` is side-level, not per-cell.

## Future Ideas

Draggable existing-road panel is possible now that the panel is EZ-owned UI. The main UX problem is finding a safe drag handle because the panel is intentionally tiny and button clicks must still work.

ZoneTools uses `react-draggable` with a title bar as the drag handle. That works there because the panel is larger and has a visible header. EZ currently has no title bar, so copying that directly would make the compact panel feel heavier.

Possible approaches:

- Add a small grip/handle above or beside the buttons.
- Add a "draggable panel" option and only show the handle when enabled.
- Let the panel snap back/reset from Options if it gets moved somewhere awkward.

Depth/width controls are also more realistic now because the existing-road panel is custom UI. Depth should probably start at 1-6 because vanilla zoning depth is normally 6 cells. Width along the road could be larger, but it needs research because vanilla blocks, terrain, intersections, and existing cells may split or clamp results.

Best place to prototype depth/width first: ZoneTools, because it already has a larger draggable panel and more room for controls. After the behavior is proven, EZ can get a compact version.
