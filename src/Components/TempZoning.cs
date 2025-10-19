// Components/TempZoning.cs
// Purpose: temporary ECS component the tool uses while previewing and before commit.
// Used by ZoningControllerToolSystem and SyncBlockSystem.

using Unity.Entities;
using Unity.Mathematics;

namespace ARTZone.Components
{
    public struct TempZoning : IComponentData
    {
        public int2 Depths;
    }
}
