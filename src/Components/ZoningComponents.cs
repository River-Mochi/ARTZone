// File: src/Components/ZoningComponents.cs
// Purpose: Holds temporary preview depths (left/right) while hovering / previewing.
// Preview = temporary overlay, Depth = real stored setting.


namespace EasyZoning.Components
{
    using Colossal.Serialization.Entities; // ISerializable, IWriter/IReader for save/load
    using Game.Prefabs;                    // CompositionFlags
    using System;                          // IEquatable
    using Unity.Entities;                  // IComponentData
    using Unity.Mathematics;               // int2

    /// <summary>
    /// Live preview depths (hover/flip) for a road entity.
    /// Depths = temporary preview, CommittedDepths = road state to restore when preview ends.
    /// </summary>
    public struct ZoningPreviewComponent : IComponentData
    {
        public int2 Depths;          // x = left, y = right
        public int2 CommittedDepths; // x = left, y = right
        public CompositionFlags CommittedFlags;
        public bool HasCommittedUpgraded;
    }

    /// <summary>
    /// One-frame restore target used when a hover preview is removed without applying.
    /// </summary>
    public struct ZoningRestoreComponent : IComponentData
    {
        public int2 Depths; // x = left, y = right
    }

    /// <summary>
    /// Committed/desired zoning depths for this road entity (per road, per side).
    /// Depths.x = left, Depths.y = right (cells).
    /// </summary>
    public struct ZoningDepthComponent :
        IComponentData,
        IEquatable<ZoningDepthComponent>,
        ISerializable
    {
        public int depthLeft;
        public int depthRight;

        public int2 Depths
        {
            readonly get => new int2(depthLeft, depthRight);
            set
            {
                depthLeft = value.x;
                depthRight = value.y;
            }
        }

        public readonly bool Equals(ZoningDepthComponent other) =>
            other.depthLeft == depthLeft && other.depthRight == depthRight;

        public readonly void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(depthLeft);
            writer.Write(depthRight);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out depthLeft);
            reader.Read(out depthRight);
        }
    }
}
