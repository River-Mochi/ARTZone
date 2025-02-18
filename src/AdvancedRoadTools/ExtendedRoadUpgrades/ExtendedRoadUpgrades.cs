// <copyright file="ExtendedRoadUpgrades.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// This code was originally part of Extended Road Upgrades by ST-Apps. It has been incorporated into this project with permission of ST-Apps.
namespace AdvancedRoadTools.ExtendedRoadUpgrades
{
    using System.Collections.Generic;
    using Game.Prefabs;

    /// <summary>
    /// Main container of all the available upgrade modes.
    /// This is not intended as an example of how to design a proper C# project so I'll
    /// just dump everything I need into a static variable and call it a day.
    ///
    /// Please don't use my code to learn how to program! :). -ST-Apps
    /// </summary>
    internal class ExtendedRoadUpgrades
    {
        /// <summary>
        ///     This variable contains all the available upgrade modes that we support.
        /// </summary>
        public static IEnumerable<ExtendedRoadUpgradeModel> Modes = new[]
        {
            // Quay
            new ExtendedRoadUpgradeModel
            {
                ObsoleteId = "ZoneControllerTool",
                Id = "Zone Controller Tool",
                m_SetUpgradeFlags = new CompositionFlags
                {
                },
                m_UnsetUpgradeFlags = new CompositionFlags
                {
                },

                // TODO: not sure how this works yet
                m_SetState = new [] { NetPieceRequirements.Edge },
                m_UnsetState = null,
                IsUnderground = true
            },
        };
    }
}
