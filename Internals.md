# Easy Zoning Internals

This file is a quick-reference for how `EasyZoning` works after the `1.5.6f1` game patch.

Version note:

- The vanilla road-side zoning FAB / `ZonesDisabled` compatibility issue in this file refers to `1.5.6f1`.
- If older notes mention another `1.5.x` patch number, re-check against the decompiled game files before assuming the road-zone behavior changed there.

It is meant to answer these questions fast:

- How does the vanilla existing-road zone FAB behave?
- Which game fields and jobs matter for road-side zoning now?
- Which EZ systems own which part of the behavior?
- What are the important "do not forget this" gotchas?

## Vanilla Existing-Road Zone FAB

This is the new vanilla road zoning FAB in the Roads Services tab.

- It lives in the same general area as crosswalk / wide sidewalk / other road FABs.
- It works on existing roads only.
- It does not handle new road drawing.

### Observed behavior

- Hovering near the left side of the road targets the left zone-cell side.
- Hovering near the right side of the road targets the right zone-cell side.
- The road midpoint matters. Hover slightly left of center to target left, slightly right to target right.
- `RMB` always adds zone cell blocks on the hovered side.
- `LMB` always disables zone cell blocks on the hovered side.
- If a side has no zone cells, vanilla shows an "add" preview.
- If a side already has zone cells, vanilla shows a "remove/disable" preview.
- The user-visible remove preview is a translucent red overlay.
- The user-visible add preview is a translucent blue/normal highlight.
- Vanilla can do `Both`, `Left`, `Right`, or `None`, but it reaches those states through side-by-side add/remove actions rather than EZ's 4-mode cycle.

### Important vanilla mental model

Vanilla is not really "placing special custom EZ depths."

Vanilla is mostly treating each road side as:

- zone cells enabled on that side
- zone cells disabled on that side

That side state now lives in upgrade flags on the road entity.

## EZ vs Vanilla

EZ and vanilla both change which side of the road gets zone cells, but they do it differently.

### Vanilla

- Existing roads only.
- Side is chosen from hover position.
- `RMB` adds.
- `LMB` disables.
- Uses vanilla temp preview roads while hovering/editing.

### EZ

- Existing roads: custom EZ tool.
- New roads: UI controls injected into the vanilla road tool panel.
- Existing roads use EZ preview + `LMB` apply + `RMB` cycle.
- New roads can already place the final side correctly, even if live side preview while dragging is still phase 2.

## Key Game Fields And APIs

These are the main game-side fields and jobs to remember.

### `Game.Net.Upgraded.m_Flags`

This is the most important patch `1.5.6f1` road zoning field.

- `Upgraded.m_Flags.m_Left`
- `Upgraded.m_Flags.m_Right`

The important bit for this mod is:

- `CompositionFlags.Side.ZonesDisabled`

Meaning:

- if left side has `ZonesDisabled`, left road-side zone cells are disabled
- if right side has `ZonesDisabled`, right road-side zone cells are disabled

This is the authoritative vanilla side-state when present.

### `Game.Tools.Temp`

Very important for not interfering with vanilla preview roads.

Relevant pieces:

- `Temp.m_Flags`
- `Temp.m_Original`
- `TempFlags.Create`

Useful mental model:

- true newly drawn roads usually have `TempFlags.Create` and `Temp.m_Original == Entity.Null`
- vanilla existing-road upgrade previews can also be temp roads, but they usually mirror an original road and therefore have `Temp.m_Original != Entity.Null`

This distinction is important because EZ must not treat every temp road as a new-road placement road.

### `Game.Zones.SubBlock`

Roads own buffers of sub-block entities.

These sub-blocks are the actual zone block entities EZ updates.

Useful for:

- finding the blocks under a road
- marking sub-blocks `Updated`
- inferring left/right current state from block layout

### `Game.Zones.Block`

Important fields:

- `Block.m_Size.y`
- `Block.m_Direction`
- `Block.m_Position`

For EZ, `Block.m_Size.y` is the zone depth on that block side.

In practice:

- `0` means disabled
- `6` means enabled with vanilla depth

### `Game.Zones.ValidArea`

Important field:

- `ValidArea.m_Area.w`

EZ keeps this aligned with `Block.m_Size.y`.

If the block says depth `6` but valid area is not aligned, behavior gets weird.

### `Game.Common.Owner`

Important field:

- `Owner.m_Owner`

This is how a zone block points back to the owning road.

### `Game.Net.Curve`

Used to classify whether a block belongs to the left or right side of a curved road.

EZ uses the road curve plus block direction instead of older rough heuristics whenever possible.

### `Game.Common.Updated`

This is the dirty marker that tells downstream game systems something changed.

If the road or sub-blocks are changed without the right `Updated` markers, previews or restores may not refresh when expected.

### `Game.Common.Created`

This matters for new roads.

Freshly placed roads and their zone blocks can still carry `Created` when EZ needs to apply the chosen side.

Do not make new-road sync depend only on `Updated`. A road/block can be fresh work that needs EZ sizing even when the specific `Updated` marker is not present at the moment our system runs.

### `Game.Common.Applied`

Vanilla apply systems add `Applied`, `Created`, and `Updated` when temp work is committed.

This is useful context when debugging timing bugs, but EZ's new-road side selection should not rely on `Applied` alone. The practical marker EZ currently needs for final fresh-road sizing is `Created` on the road or block, plus EZ's own stored `ZoningDepthComponent`.

## Important Decompiled Vanilla Jobs

These are worth remembering in the local decompile repo at:

- `C:\Users\kldan\source\repos\research`

### `Game.Zones.ZoneToggleJob`

File:

- `research/Game.Zones/ZoneToggleJob.cs`

Why it matters:

- This is where vanilla reads `Upgraded.m_Flags.m_Left/m_Right`
- It checks `CompositionFlags.Side.ZonesDisabled`
- It blocks cells on whichever side is disabled

This is one of the clearest proofs that road-side zoning is now upgrade-flag driven.

### `Game.Zones.ZoneCellHighlightJob`

File:

- `research/Game.Zones/ZoneCellHighlightJob.cs`

Why it matters:

- This is the highlight/previews path for vanilla temp roads
- It reads the temp road's `Upgraded` flags
- It compares block direction against road geometry to determine left/right side
- It highlights only the side currently disabled in the preview temp road

This is a big reason EZ must stay away from vanilla temp existing-road previews.

### `Game.Zones.CellCheckSystem`

File:

- `research/Game.Zones/CellCheckSystem.cs`

Why it matters:

- Schedules `ZoneToggleJob`
- Sits in the road-zone-cell update pipeline

When debugging "why did the cells end up like this," this is one of the main systems worth reopening.

## EZ Systems And Responsibilities

### `src/Tools/ZoningControllerToolSystem.cs`

This is the EZ existing-road tool.

Responsibilities:

- road hover selection
- preview state
- `RMB` cycle
- `LMB` apply
- storing temporary preview state separately from committed state
- writing final road-side state back to `Upgraded.m_Flags`

Important current ideas in that file:

- preview should never be mistaken for committed state
- when preview ends, restore the original road-side state
- when tool exits, clean up transient preview state

### `src/Tools/SyncBlockSystem.cs`

This is the block sync system.

Responsibilities:

- read effective road-side state
- update block depth and valid area
- skip temp roads that belong to vanilla existing-road upgrade previews
- finish applying EZ-selected side depth to freshly created new-road blocks

Current important rule:

- if a temp road has `Temp.m_Original != Entity.Null` and EZ is not actively previewing it, EZ should leave it alone

That rule is one of the main protections against vanilla FAB interference.

New-road rule:

- allow block sync for freshly created roads only when a zonable vanilla `NetToolSystem` road-build tool is active
- the road must carry EZ's `ZoningDepthComponent`
- either the road or block should still carry `Created`
- this keeps new-road side selection working without letting EZ side mode affect vanilla existing-road FAB previews

### `src/Tools/SyncNewRoadsSystem.cs`

This is the new-road sync system.

Responsibilities:

- apply EZ road-side state to freshly drawn roads
- keep true new placement roads in sync while drawing
- write compatible `Upgraded` side flags for new roads too

Current important rule:

- only treat temp roads as "new roads" if they are true created roads and `Temp.m_Original == Entity.Null`
- do not require `Updated` on the new-road temp query; `Created` + `TempFlags.Create` + `Temp.m_Original == Entity.Null` is the important true-new-road signal

That prevents existing-road vanilla preview clones from being mistaken for fresh road placement.

Important pipeline:

1. UI writes the selected new-road mode to `RoadZoningMode`.
2. `SyncNewRoadsSystem` converts that to `(leftDepth, rightDepth)`.
3. True new-road temp entities get `ZoningDepthComponent` when the selected mode is not vanilla `Both`.
4. True new-road temp entities also get compatible `Upgraded.m_Flags` / `ZonesDisabled` so vanilla state stays aligned.
5. `SyncBlockSystem` finishes the actual block sizing on fresh created blocks.

### `src/Tools/RoadZoneCompatibility.cs`

Shared compatibility helpers.

Responsibilities:

- convert between disabled-side flags and EZ `int2` depths
- decide left/right side from block + curve
- keep the "vanilla depth means enabled" rules in one place

### `src/Tools/ZoneControlBridgeUI.cs`

UI bridge between React and C#.

Responsibilities:

- stores `ToolZoningMode`
- stores `RoadZoningMode`
- exposes bindings to the UI
- converts UI mode bits into `(leftDepth, rightDepth)`

## EZ Internal States To Remember

EZ effectively works with these road-side depth pairs:

- `Both` = `(6, 6)`
- `Left` = `(6, 0)`
- `Right` = `(0, 6)`
- `None` = `(0, 0)`

The current code treats `6` as the vanilla enabled depth.

## Gotchas

### 1. Preview state must not overwrite committed state

If the tool reads back its own preview as though it were the road's real state, hover starts flickering or roads can appear to "apply on hover."

That is why EZ now keeps committed state beside preview state.

### 2. Block layout fallback is useful, but dangerous in the wrong order

If `Upgraded` flags explicitly say a side is disabled, trust that first.

If vanilla flags are clear, block layout can be used as fallback.

If old legacy EZ data still exists, block layout should usually beat stale legacy depth data.

### 3. Vanilla existing-road previews use temp entities too

Not every temp road is a new road.

This is the easiest mistake to make when syncing EZ state into temp roads.

### 4. Road and sub-block `Updated` markers matter

Refreshing only the road entity is often not enough.

If sub-blocks are not dirtied too, visible zone-cell state can lag behind the intended state.

### 5. New-road live side preview is a separate feature

The mod already places the correct final side for new roads.

Showing side-aware zone-cell preview while dragging is possible, but it is separate work because it needs a live temp-road preview path rather than just final apply sync.

### 6. New-road final apply needs both systems

`SyncNewRoadsSystem` stores the selected side mode on the new road, but `SyncBlockSystem` does the final zone-block depth sizing.

If left/right/none icons stop working for new roads, check both systems together.

The bug fixed on `fix/new-roads` happened because EZ correctly stored the new-road mode, but the block sync safety gate was too strict after the vanilla FAB compatibility work.

The safety gate must protect vanilla existing-road upgrade previews while still allowing fresh created new-road blocks to sync.

## Good Debug Checklist

If something feels wrong, check these in order:

1. Is the road a true new road temp or an existing-road vanilla temp clone?
2. Does the road currently have `Upgraded.m_Flags` with `ZonesDisabled` on either side?
3. Is EZ accidentally reading preview state instead of committed state?
4. Are `Block.m_Size.y` and `ValidArea.m_Area.w` aligned?
5. Were `Updated` markers added to the road and its sub-blocks?
6. For new roads, did `SyncNewRoadsSystem` add/update `ZoningDepthComponent` before `SyncBlockSystem` tried to size blocks?
7. For new roads, is `SyncBlockSystem` allowed because a zonable `NetToolSystem` road-build tool is active and the road/block is `Created`?
8. For vanilla FAB interop, is EZ staying out of `UpgradeToolSystem` temp previews?

## Handy File List

- `src/Tools/ZoningControllerToolSystem.cs`
- `src/Tools/SyncBlockSystem.cs`
- `src/Tools/SyncNewRoadsSystem.cs`
- `src/Tools/RoadZoneCompatibility.cs`
- `src/Tools/ZoneControlBridgeUI.cs`
- `src/Utils/LogUtils.cs`
- `research/Game.Zones/ZoneToggleJob.cs`
- `research/Game.Zones/ZoneCellHighlightJob.cs`
- `research/Game.Zones/CellCheckSystem.cs`
