using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedRoadTools.Core;

public struct TempZoning : IComponentData
{
    public int2 Depths;
}