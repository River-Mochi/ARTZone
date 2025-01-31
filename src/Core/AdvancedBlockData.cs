using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedRoadTools.Core;

public struct AdvancedBlockData : IComponentData
{
    public float3 originalPosition;
    public int depthLeft;
    public int depthRight;
    
}