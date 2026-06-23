// <copyright file="PreviewColorOverrideSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: src/Tools/PreviewColorOverrideSystem.cs
// Purpose: Improves remove-preview visibility by overriding vanilla
// highlight edge/fill colors after ZoneSystem builds its shader color arrays.

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
        private const float kOrangeFillSaturation = 0.70f;
        private const float kRedHue = 0.00f;
        private const float kRedSaturation = 0.95f;
        private const float kRedValue = 1.00f;
        private const float kPinkHue = 0.92f;
        private const float kPinkSaturation = 0.90f;
        private const float kPinkValue = 1.00f;
        private const float kPinkFillSaturation = 0.62f;
        private const float kPurpleHue = 0.76f;
        private const float kPurpleSaturation = 0.85f;
        private const float kPurpleValue = 1.00f;
        private const float kPurpleFillSaturation = 0.58f;
        private const string kZoneEdgeShaderProperty = "colossal_ZoneEdgeColors";
        private const string kZoneFillShaderProperty = "colossal_ZoneFillColors";

        private PrefabSystem m_PrefabSystem = null!;
        private ZoneSystem m_ZoneSystem = null!;
        private EntityQuery m_ZoneQuery;

        private FieldInfo? m_EdgeColorArrayField;
        private FieldInfo? m_FillColorArrayField;
        private int m_ZoneEdgeShaderId;
        private int m_ZoneFillShaderId;
        private bool m_MissingEdgeFieldWarned;
        private bool m_MissingFillFieldWarned;

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
            m_FillColorArrayField = typeof(ZoneSystem).GetField(
                "m_FillColorArray",
                BindingFlags.Instance | BindingFlags.NonPublic);
            m_ZoneEdgeShaderId = Shader.PropertyToID(kZoneEdgeShaderProperty);
            m_ZoneFillShaderId = Shader.PropertyToID(kZoneFillShaderProperty);
        }

        protected override void OnUpdate( )
        {
            if (m_ZoneQuery.IsEmptyIgnoreFilter)
                return;

            Vector4[]? edgeColors = m_EdgeColorArrayField?.GetValue(m_ZoneSystem) as Vector4[];
            Vector4[]? fillColors = m_FillColorArrayField?.GetValue(m_ZoneSystem) as Vector4[];

            if (edgeColors == null)
            {
                if (!m_MissingEdgeFieldWarned)
                {
                    m_MissingEdgeFieldWarned = true;
                    Mod.WarnOnce(
                        "PreviewColorOverrideSystem.EdgeArrayMissing",
                        ( ) => "[EZ] PreviewColorOverrideSystem could not read ZoneSystem edge color array.");
                }
            }

            if (fillColors == null)
            {
                if (!m_MissingFillFieldWarned)
                {
                    m_MissingFillFieldWarned = true;
                    Mod.WarnOnce(
                        "PreviewColorOverrideSystem.FillArrayMissing",
                        ( ) => "[EZ] PreviewColorOverrideSystem could not read ZoneSystem fill color array.");
                }
            }

            if (edgeColors == null && fillColors == null)
                return;

            string borderStyle = Mod.Settings?.RemovePreviewBorderStyle ?? Setting.kRemovePreviewBorderOrange;
            float edgeAlpha = math.saturate((Mod.Settings?.RemovePreviewEdgeOpacityPercent ?? 100) / 100f);
            string fillStyle = Mod.Settings?.RemovePreviewFillStyle ?? Setting.kRemovePreviewFillNone;
            float fillAlpha = math.saturate((Mod.Settings?.RemovePreviewFillOpacityPercent ?? 100) / 100f);
            bool edgeChanged = false;
            bool fillChanged = false;

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

                    if (edgeColors != null && (uint) colorIndex < (uint) edgeColors.Length)
                    {
                        Color desiredEdge = BuildHighlightEdge(borderStyle, zonePrefab.m_Edge, edgeAlpha);

                        if (!Approximately(edgeColors[colorIndex], desiredEdge))
                        {
                            edgeColors[colorIndex] = desiredEdge;
                            edgeChanged = true;
                        }
                    }

                    if (fillColors != null && (uint) colorIndex < (uint) fillColors.Length)
                    {
                        Color desiredFill = BuildHighlightFill(fillStyle, zonePrefab.m_Color, fillAlpha);

                        if (!Approximately(fillColors[colorIndex], desiredFill))
                        {
                            fillColors[colorIndex] = desiredFill;
                            fillChanged = true;
                        }
                    }
                }
            }
            finally
            {
                prefabs.Dispose();
                zones.Dispose();
            }

            if (edgeChanged && edgeColors != null)
                Shader.SetGlobalVectorArray(m_ZoneEdgeShaderId, edgeColors);

            if (fillChanged && fillColors != null)
                Shader.SetGlobalVectorArray(m_ZoneFillShaderId, fillColors);
        }

        private static Color BuildOrangeHighlightEdge(float alpha)
        {
            Color color = Color.HSVToRGB(kOrangeHue, kOrangeSaturation, kOrangeValue);
            color.a = alpha;
            return color;
        }

        private static Color BuildRedHighlightEdge(float alpha)
        {
            Color color = Color.HSVToRGB(kRedHue, kRedSaturation, kRedValue);
            color.a = alpha;
            return color;
        }

        private static Color BuildPinkHighlightEdge(float alpha)
        {
            Color color = Color.HSVToRGB(kPinkHue, kPinkSaturation, kPinkValue);
            color.a = alpha;
            return color;
        }

        private static Color BuildPurpleHighlightEdge(float alpha)
        {
            Color color = Color.HSVToRGB(kPurpleHue, kPurpleSaturation, kPurpleValue);
            color.a = alpha;
            return color;
        }

        private static Color BuildHighlightEdge(string borderStyle, Color edgeColor, float opacityPercent)
        {
            switch (borderStyle)
            {
                case Setting.kRemovePreviewBorderRed:
                    return BuildRedHighlightEdge(opacityPercent);

                case Setting.kRemovePreviewBorderPink:
                    return BuildPinkHighlightEdge(opacityPercent);

                case Setting.kRemovePreviewBorderPurple:
                    return BuildPurpleHighlightEdge(opacityPercent);

                case Setting.kRemovePreviewBorderVanillaRed:
                {
                    Color vanilla = BuildVanillaHighlightEdge(edgeColor);
                    vanilla.a *= opacityPercent;
                    return vanilla;
                }

                default:
                    return BuildOrangeHighlightEdge(opacityPercent);
            }
        }

        private static Color BuildVanillaHighlightEdge(Color edgeColor)
        {
            Color.RGBToHSV(edgeColor, out _, out _, out float value);
            Color highlight = Color.HSVToRGB(0f, 0.85f, value);
            highlight.a = math.min(1f, edgeColor.a * 1.25f);
            return highlight;
        }

        private static Color BuildHighlightFill(string fillStyle, Color baseFillColor, float opacityPercent)
        {
            Color vanillaFill = BuildVanillaHighlightFill(baseFillColor);

            switch (fillStyle)
            {
                case Setting.kRemovePreviewFillWhite:
                {
                    Color white = Color.white;
                    white.a = vanillaFill.a * opacityPercent;
                    return white;
                }

                case Setting.kRemovePreviewFillOrange:
                {
                    Color orange = Color.HSVToRGB(kOrangeHue, kOrangeFillSaturation, kOrangeValue);
                    orange.a = vanillaFill.a * opacityPercent;
                    return orange;
                }

                case Setting.kRemovePreviewFillPink:
                {
                    Color pink = Color.HSVToRGB(kPinkHue, kPinkFillSaturation, kPinkValue);
                    pink.a = vanillaFill.a * opacityPercent;
                    return pink;
                }

                case Setting.kRemovePreviewFillPurple:
                {
                    Color purple = Color.HSVToRGB(kPurpleHue, kPurpleFillSaturation, kPurpleValue);
                    purple.a = vanillaFill.a * opacityPercent;
                    return purple;
                }

                case Setting.kRemovePreviewFillNone:
                {
                    vanillaFill.a = 0f;
                    return vanillaFill;
                }

                default:
                    vanillaFill.a *= opacityPercent;
                    return vanillaFill;
            }
        }

        private static Color BuildVanillaHighlightFill(Color fillColor)
        {
            Color.RGBToHSV(fillColor, out _, out _, out float value);
            Color highlight = Color.HSVToRGB(0f, 0.85f, value);
            highlight.a = math.min(1f, fillColor.a * 1.25f);
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
