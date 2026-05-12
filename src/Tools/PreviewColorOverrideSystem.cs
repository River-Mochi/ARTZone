// File: src/Tools/PreviewColorOverrideSystem.cs
// Purpose: Improves remove-preview visibility by overriding the vanilla
// highlight edge color after ZoneSystem builds its shader color arrays.

namespace EasyZoning.Tools
{
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Zones;
    using System.Reflection;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public partial class PreviewColorOverrideSystem : GameSystemBase
    {
        private const float kOrangeHue = 0.08f;
        private const float kOrangeSaturation = 0.95f;
        private const float kOrangeValue = 1.00f;
        private const string kZoneEdgeShaderProperty = "colossal_ZoneEdgeColors";

        private PrefabSystem m_PrefabSystem = null!;
        private ZoneSystem m_ZoneSystem = null!;
        private EntityQuery m_ZoneQuery;

        private FieldInfo? m_EdgeColorArrayField;
        private int m_ZoneEdgeShaderId;
        private bool m_MissingFieldWarned;

        protected override void OnCreate( )
        {
            base.OnCreate();

            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ZoneSystem = World.GetOrCreateSystemManaged<ZoneSystem>();

            m_ZoneQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ZoneData, PrefabData>()
                .WithNone<Deleted>()
                .Build(this);

            m_EdgeColorArrayField = typeof(ZoneSystem).GetField(
                "m_EdgeColorArray",
                BindingFlags.Instance | BindingFlags.NonPublic);
            m_ZoneEdgeShaderId = Shader.PropertyToID(kZoneEdgeShaderProperty);
        }

        protected override void OnUpdate( )
        {
            if (m_ZoneQuery.IsEmptyIgnoreFilter)
                return;

            if (m_EdgeColorArrayField?.GetValue(m_ZoneSystem) is not Vector4[] edgeColors)
            {
                if (!m_MissingFieldWarned)
                {
                    m_MissingFieldWarned = true;
                    Mod.WarnOnce(
                        "PreviewColorOverrideSystem.EdgeArrayMissing",
                        ( ) => "[EZ] PreviewColorOverrideSystem could not read ZoneSystem edge color array.");
                }

                return;
            }

            bool useOrangeEdge = Mod.Settings?.UseOrangeRemovePreviewEdge ?? true;
            float edgeAlpha = math.saturate((Mod.Settings?.RemovePreviewEdgeOpacityPercent ?? 100) / 100f);
            bool changed = false;

            NativeArray<PrefabData> prefabs = m_ZoneQuery.ToComponentDataArray<PrefabData>(Allocator.Temp);
            NativeArray<ZoneData> zones = m_ZoneQuery.ToComponentDataArray<ZoneData>(Allocator.Temp);

            try
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    ZonePrefab zonePrefab = m_PrefabSystem.GetPrefab<ZonePrefab>(prefabs[i]);
                    int colorIndex = ZoneUtils.GetColorIndex(
                        CellFlags.Visible | CellFlags.Highlight,
                        zones[i].m_ZoneType);

                    if ((uint) colorIndex >= (uint) edgeColors.Length)
                        continue;

                    Color desired = useOrangeEdge
                        ? BuildOrangeHighlightEdge(edgeAlpha)
                        : BuildVanillaHighlightEdge(zonePrefab.m_Edge);

                    if (!Approximately(edgeColors[colorIndex], desired))
                    {
                        edgeColors[colorIndex] = desired;
                        changed = true;
                    }
                }
            }
            finally
            {
                prefabs.Dispose();
                zones.Dispose();
            }

            if (changed)
                Shader.SetGlobalVectorArray(m_ZoneEdgeShaderId, edgeColors);
        }

        private static Color BuildOrangeHighlightEdge(float alpha)
        {
            Color color = Color.HSVToRGB(kOrangeHue, kOrangeSaturation, kOrangeValue);
            color.a = alpha;
            return color;
        }

        private static Color BuildVanillaHighlightEdge(Color edgeColor)
        {
            Color.RGBToHSV(edgeColor, out _, out _, out float value);
            Color highlight = Color.HSVToRGB(0f, 0.85f, value);
            highlight.a = math.min(1f, edgeColor.a * 1.25f);
            return highlight;
        }

        private static bool Approximately(Vector4 current, Color desired)
        {
            return math.abs(current.x - desired.r) < 0.001f &&
                   math.abs(current.y - desired.g) < 0.001f &&
                   math.abs(current.z - desired.b) < 0.001f &&
                   math.abs(current.w - desired.a) < 0.001f;
        }
    }
}
