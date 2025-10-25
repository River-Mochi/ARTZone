// Components/TempZoning.cs
// Purpose: temporary component the tool uses while previewing and before commit. ZoningControllerToolSystem and SyncBlockSystem reference this.
using Unity.Entities;
using Unity.Mathematics;

namespace ARTZone.Components
{

    public struct TempZoning : IComponentData
    {
        public int2 Depths;
    }
}
