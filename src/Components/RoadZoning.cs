// File: src/Components/RoadZoning.cs
// Purpose: Persistent per-road zoning depths (left/right). Applied by SyncBlockSystem.

namespace ARTZone.Components
{
    using System;
    using Colossal.Serialization.Entities;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct RoadZoning : IComponentData, IEquatable<RoadZoning>, ISerializable
    {
        public int depthLeft;
        public int depthRight;

        public int2 Depths
        {
            get => new int2(depthLeft, depthRight);
            set
            {
                depthLeft = value.x;
                depthRight = value.y;
            }
        }

        public bool Equals(RoadZoning other) => other.depthLeft == depthLeft && other.depthRight == depthRight;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
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
