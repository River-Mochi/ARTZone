// <copyright file="ZoningMode.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: src/Tools/ZoningMode.cs
// Purpose: Bitmask used by UI and tool systems for left/right/both zoning toggles.
namespace EasyZoning.Tools
{
    using System;   // Flags

    [Flags]
    public enum ZoningMode
    {
        None = 0,
        Right = 1,
        Left = 2,
        Both = Right | Left
    }
}
