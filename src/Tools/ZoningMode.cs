// src/Tools/ZoningMode.cs
// Bitmask used by UI and tool systems for left/right/both zoning toggles.

namespace AdvancedRoadTools
{
    using System;

    [Flags]
    public enum ZoningMode
    {
        None = 0,
        Right = 1,
        Left = 2,
        Both = Right | Left
    }
}
