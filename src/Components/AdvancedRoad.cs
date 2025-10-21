// Components/AdvancedRoad.cs
namespace AdvancedRoadTools.Components
{
    using System;
    using Colossal.Serialization.Entities;
    using Unity.Entities;
    using Unity.Mathematics;
    public struct AdvancedRoad : IComponentData, IEquatable<AdvancedRoad>, ISerializable
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

        public bool Equals(AdvancedRoad other) => other.depthLeft == depthLeft && other.depthRight == depthRight;

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
