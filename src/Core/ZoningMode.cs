using System;

namespace AdvancedRoadTools.Core;

[Flags]
public enum ZoningMode
{
    None = 0,
    Right = 1,
    Left = 2,
    Both = 4
}