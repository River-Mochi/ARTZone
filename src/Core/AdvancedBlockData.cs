using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedRoadTools.Core;

public struct AdvancedBlockData : IComponentData
{
    public float3 originalPosition;
    public float3 offset;
    public int depth;
    public Setting.TillingModes TillingMode;
}