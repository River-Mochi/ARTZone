// File: src/ARTZone/Tools/RoadRmbInvertSystem.cs
// Purpose: While the vanilla Net Tool is active (placing roads), let RMB invert the road zoning mode.

namespace ARTZone.Tools
{
    using Game;                 // GameSystemBase
    using Game.Tools;           // ToolSystem, ToolBaseSystem
    using Game.Prefabs;         // RoadPrefab
    using Game.Input;           // ProxyAction
    using Unity.Entities;       // World

    public sealed partial class RoadRmbInvertSystem : GameSystemBase
    {
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolUISystem m_UISystem = null!;
        private ProxyAction? m_Invert;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Invert = ARTZoneMod.m_InvertZoningAction; // already enabled in Mod.OnLoad
        }

        protected override void OnUpdate()
        {
            if (m_Invert == null || !m_Invert.WasPressedThisFrame())
                return;

            var active = m_MainToolSystem.activeTool as ToolBaseSystem;
            var prefab = active?.GetPrefab();

            // Only when the vanilla road placement is in control (i.e., a RoadPrefab is active)
            if (prefab is RoadPrefab)
            {
                var current = m_UISystem.RoadZoningMode; // public getter already exists
                var inverted = (ZoningMode)((int)current ^ (int)ZoningMode.Both); // flip Left/Right bits
                m_UISystem.SetRoadZoningMode(inverted);
            }
        }
    }
}
