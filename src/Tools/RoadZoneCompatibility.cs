// File: src/Tools/RoadZoneCompatibility.cs
// Purpose: Shared helpers for CS2 1.5.6f1 road zoning compatibility.

namespace EasyZoning.Tools
{
    using Colossal.Mathematics; // MathUtils
    using Game.Net;             // Curve
    using Game.Prefabs;         // CompositionFlags
    using Game.Zones;           // Block
    using Unity.Mathematics;    // float2, int2, math

    internal static class RoadZoneCompatibility
    {
        internal const int EnabledDepth = 6;
        private const float SideEpsilon = 0.01f;

        internal static readonly int2 VanillaDepths = new int2(EnabledDepth, EnabledDepth);

        internal static bool TryGetDepthsFromFlags(CompositionFlags flags, out int2 depths)
        {
            bool leftDisabled = (flags.m_Left & CompositionFlags.Side.ZonesDisabled) != 0;
            bool rightDisabled = (flags.m_Right & CompositionFlags.Side.ZonesDisabled) != 0;

            if (!leftDisabled && !rightDisabled)
            {
                depths = default;
                return false;
            }

            depths = DepthsFromDisabledSides(leftDisabled, rightDisabled);
            return true;
        }

        internal static int2 DepthsFromDisabledSides(bool leftDisabled, bool rightDisabled)
        {
            return new int2(
                leftDisabled ? 0 : EnabledDepth,
                rightDisabled ? 0 : EnabledDepth);
        }

        internal static CompositionFlags ApplyDepthsToFlags(CompositionFlags flags, int2 depths)
        {
            flags.m_Left = SetZonesDisabled(flags.m_Left, depths.x <= 0);
            flags.m_Right = SetZonesDisabled(flags.m_Right, depths.y <= 0);
            return flags;
        }

        internal static bool HasAnyFlags(CompositionFlags flags) => flags != default;

        internal static bool IsBlockOnLeft(Block block, Curve curve)
        {
            float dot = GetBlockCurveDotProduct(block, curve);
            if (dot > SideEpsilon)
                return true;

            if (dot < -SideEpsilon)
                return false;

            return math.dot(new float2(1f, 1f), block.m_Direction) < 0f;
        }

        internal static float GetBlockCurveDotProduct(Block block, Curve curve)
        {
            MathUtils.Distance(curve.m_Bezier.xz, block.m_Position.xz, out float t);

            float oneMinusT = 1f - t;
            float2 tangent =
                3f * oneMinusT * oneMinusT * (curve.m_Bezier.xz.b - curve.m_Bezier.xz.a) +
                6f * oneMinusT * t * (curve.m_Bezier.xz.c - curve.m_Bezier.xz.b) +
                3f * t * t * (curve.m_Bezier.xz.d - curve.m_Bezier.xz.c);

            tangent = math.normalizesafe(tangent);
            if (math.lengthsq(tangent) <= 1E-07f)
                return 0f;

            float2 perpendicular = new float2(tangent.y, -tangent.x);
            return math.dot(perpendicular, block.m_Direction);
        }

        private static CompositionFlags.Side SetZonesDisabled(CompositionFlags.Side side, bool disabled)
        {
            if (disabled)
                return side | CompositionFlags.Side.ZonesDisabled;

            return side & ~CompositionFlags.Side.ZonesDisabled;
        }
    }
}
