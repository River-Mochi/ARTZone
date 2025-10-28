// File: src/Tools/ZoningControllerToolSystem.cs
// Purpose:
//   Runtime tool. RMB toggles preview over valid roads (Left<->Right or Both<->None).
//   LMB confirms. ESC is not handled here (left to vanilla UI to close panels).
//   Preview always reflects the current mode for the hovered segment.

namespace ARTZone.Tools
{
    using System;
    using ARTZone.Components;
    using Game.Audio;
    using Game.Common;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;

    public partial class ZoningControllerToolSystem : ToolBaseSystem
    {
        // DO NOT CHANGE ToolID string. It must match every UI/TS reference
        public const string ToolID = "ARTZone.ZoningTool";      // if this changes, MUST change also in TS.
        public override string toolID => ToolID;    // required

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ZoningControllerToolUISystem m_UISystem = null!;
        private ToolHighlightSystem m_Highlight = null!;

        private BufferLookup<SubBlock> m_SubBlockLookup;

        private EntityQuery m_ZoningPreviewQuery;
        private EntityQuery m_SoundbankQuery;

        private PrefabBase m_ToolPrefab = null!;

        private NativeList<Entity> m_SelectedEntities;

        private enum Mode
        {
            None, Select, Apply, Preview
        }
        private Mode m_Mode;
        private Entity m_PreviewEntity;

        private int2 Depths => m_UISystem.ToolDepths;

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = ARTZoneMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[ART][Tool] " + msg);
            }
            catch { }
        }
#else
        private static void Dbg(string msg)
        {
        }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Highlight = World.GetOrCreateSystemManaged<ToolHighlightSystem>();

            m_ZoningPreviewQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ZoningPreviewComponent>()
                .Build(this);

            m_SoundbankQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ToolUXSoundSettingsData>()
                .Build(this);

            m_SubBlockLookup = GetBufferLookup<SubBlock>(true);

            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            if (m_SelectedEntities.IsCreated)
                m_SelectedEntities.Dispose();
            base.OnDestroy();
        }

        // ----  Lifecycle ------------------------------------------------------------
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            applyAction.enabled = true;
            cancelAction.enabled = false;   // Let Esc bubble to vanilla UI
            requireZones = true;
            requireNet = Layer.Road;
            allowUnderground = true;
#if DEBUG
            Dbg("OnStartRunning: tool ACTIVE");
#endif
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            applyAction.enabled = false;
            cancelAction.enabled = false;
            requireZones = false;
            requireNet = Layer.None;
            allowUnderground = false;
#if DEBUG
            Dbg("OnStopRunning: tool INACTIVE");
#endif
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_SubBlockLookup.Update(this);
            inputDeps = Dependency;

            bool hasRoad;
            Entity hitEntity;
            RaycastHit hit;
            try
            {
                hasRoad = TryGetRoadUnderCursor(out hitEntity, out hit);
            }
            catch { hasRoad = false; hitEntity = Entity.Null; }

            // UI vanilla sounds
            var haveSoundbank = m_SoundbankQuery.CalculateEntityCount() > 0;
            ToolUXSoundSettingsData soundbank = default;
            if (haveSoundbank)
                soundbank = m_SoundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

            // RMB toggling (raw). ESC is NOT handled here so vanilla can close panels.
            bool rmbPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

            if (rmbPressed && hasRoad)
            {
                // Ensure hovered entity is selected so the flip previews immediately
                if (m_PreviewEntity == Entity.Null || m_PreviewEntity != hitEntity)
                {
                    if (m_PreviewEntity != Entity.Null)
                        m_Highlight.HighlightEntity(m_PreviewEntity, false);

                    m_SelectedEntities.Clear();
                    m_Highlight.HighlightEntity(hitEntity, true);
                    m_SelectedEntities.Add(hitEntity);
                    m_PreviewEntity = hitEntity;
                }
                else if (!m_SelectedEntities.Contains(hitEntity))
                {
                    m_SelectedEntities.Add(hitEntity);
                    m_Highlight.HighlightEntity(hitEntity, true);
                }

                // Left<->Right when on a side; otherwise Both<->None
                m_UISystem.RmbPreviewToggle();

                if (haveSoundbank)
                    AudioManager.instance.PlayUISound(soundbank.m_SnapSound);

                m_Mode = Mode.Preview; // LMB confirms
            }
            else if (applyAction.WasPressedThisFrame() || applyAction.IsPressed())
            {
                m_Mode = Mode.Select;
            }
            else if (applyAction.WasReleasedThisFrame() && hasRoad)
            {
                m_Mode = Mode.Apply;
            }
            else
            {
                m_Mode = Mode.Preview;
            }

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            switch (m_Mode)
            {
                case Mode.Preview:
                    if (m_PreviewEntity != hitEntity)
                    {
                        if (m_PreviewEntity != Entity.Null)
                            m_Highlight.HighlightEntity(m_PreviewEntity, false);

                        m_SelectedEntities.Clear();
                        m_PreviewEntity = Entity.Null;

                        if (hasRoad)
                        {
                            m_Highlight.HighlightEntity(hitEntity, true);
                            m_SelectedEntities.Add(hitEntity);
                            m_PreviewEntity = hitEntity;
                        }
                    }
                    break;

                case Mode.Select when hasRoad:
                    if (!m_SelectedEntities.Contains(hitEntity))
                    {
                        m_SelectedEntities.Add(hitEntity);
                        m_Highlight.HighlightEntity(hitEntity, true);
                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
                    }
                    break;

                case Mode.Apply:
                    {
                        JobHandle setJob = new SetZoningDepthJob
                        {
                            ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(true),
                            Entities = m_SelectedEntities.AsArray().AsReadOnly(),
                            ECB = ecb
                        }.Schedule(inputDeps);

                        inputDeps = JobHandle.CombineDependencies(inputDeps, setJob);

                        for (var i = 0; i < m_SelectedEntities.Length; i++)
                            m_Highlight.HighlightEntity(m_SelectedEntities[i], false);
                        m_SelectedEntities.Clear();

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);
                        break;
                    }
            }

            var tempLookup = GetComponentLookup<ZoningPreviewComponent>(true);

            JobHandle syncTempJob = new SyncTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                ZoningPreviewLookup = tempLookup,
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Depths = Depths
            }.Schedule(m_SelectedEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, syncTempJob);

            NativeArray<Entity> zoningPreviewEntities = m_ZoningPreviewQuery.ToEntityArray(Allocator.TempJob);

            JobHandle cleanupTempJob = new CleanupTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Entities = zoningPreviewEntities.AsReadOnly()
            }.Schedule(zoningPreviewEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, cleanupTempJob);
            zoningPreviewEntities.Dispose(inputDeps);

            m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        // Helper - returns true if the cursor is over a zonable road entity.
        // Roads without SubBlocks (e.g., highways) are ignored.
        private bool TryGetRoadUnderCursor(out Entity entity, out RaycastHit hit)
        {
            if (!GetRaycastResult(out entity, out hit))
                return false;
            // filter gate only accepts roads that expose zone SubBlocks
            if (!m_SubBlockLookup.TryGetBuffer(entity, out _))
            {
                entity = Entity.Null;
                return false;
            }
            return true;        // zonable road entitires pass to preview/apply
        }

        public override PrefabBase GetPrefab() => m_ToolPrefab;


        // ----  Prefab ownership (called by PanelBuilder) --------------------------
        public override bool TrySetPrefab(PrefabBase prefab)
        {
            if (prefab == null || prefab.name != toolID)
            {
#if DEBUG
                ARTZoneMod.s_Log.Warn($"[ART][Tool] TrySetPrefab rejected: prefab='{prefab?.name}', expected='{ToolID}'");
#endif
                return false;
            }
            m_ToolPrefab = prefab;
#if DEBUG
            Dbg($"TrySetPrefab: accepted prefab='{prefab.name}'");
#endif
            return true;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.netLayerMask = Layer.Road;
        }

        // ----  Toggle from UI/Hotkey -----------------------------------------------
        public void SetToolEnabled(bool isEnabled)
        {
            if (m_ToolSystem == null)
                return;

            if (isEnabled && m_ToolSystem.activeTool != this)
            {
#if DEBUG
                Dbg("SetToolEnabled(true): Activating our tool");
#endif
                m_ToolSystem.ActivatePrefabTool(GetPrefab());
            }
            else if (!isEnabled && m_ToolSystem.activeTool == this)
            {
#if DEBUG
                Dbg("SetToolEnabled(false): Deactivating our tool");
#endif
                m_ToolSystem.ActivatePrefabTool(null);
            }
        }

        public struct SyncTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public int2 Depths;

            // Avoid redundant AddComponent when preview depths unchanged
            public void Execute(int index)
            {
                Entity e = SelectedEntities[index];

                if (ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent data))
                {
                    if (!math.all(data.Depths == Depths))
                    {
                        ECB.SetComponent(index, e, new ZoningPreviewComponent { Depths = Depths });
                    }
                    // else: already matches; no-op
                }
                else
                {
                    ECB.AddComponent(index, e, new ZoningPreviewComponent { Depths = Depths });
                }

                ECB.AddComponent<Updated>(index, e);
            }

        }

        public struct CleanupTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public NativeArray<Entity>.ReadOnly Entities;

            public void Execute(int index)
            {
                Entity e = Entities[index];
                if (SelectedEntities.Contains(e))
                    return;

                ECB.RemoveComponent<ZoningPreviewComponent>(index, e);
                ECB.AddComponent<Updated>(index, e);
            }
        }

        public struct SetZoningDepthJob : IJob
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            public EntityCommandBuffer ECB;

            public void Execute()
            {
                foreach (Entity e in Entities)
                {
                    if (!ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent temp))
                        continue;

                    ECB.RemoveComponent<ZoningPreviewComponent>(e);
                    ECB.AddComponent(e, new ZoningDepthComponent { Depths = temp.Depths });
                }
            }
        }
    }
}
