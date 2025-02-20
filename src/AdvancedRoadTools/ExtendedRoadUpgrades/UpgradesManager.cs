// <copyright file="UpgradesManager.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// This code was originally part of Extended Road Upgrades by ST-Apps. It has been incorporated into this project with permission of ST-Apps.

using AdvancedRoadTools.Core;
using AdvancedRoadTools.Core.Logging;

namespace AdvancedRoadTools.ExtendedRoadUpgrades
{
    using System.Collections.Generic;
    using System.Linq;
    using Colossal.Json;
    using Game.Prefabs;
    using Game.SceneFlow;
    using HarmonyLib;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// This class provides the utility methods to install the mod.
    /// </summary>
    internal static class UpgradesManager
    {
        /// <summary>
        ///     Base URI for all of our icons.
        /// </summary>
        private static readonly string COUIBaseLocation = $"coui://uil/Standard/RoadUpgrade";

        /// <summary>
        ///     Guard boolean used to check if the Prefix already executed, so that we can prevent executing it multiple times.
        /// </summary>
        private static bool installed;

        /// <summary>
        ///     Guard boolean used to check if the Event Handler already executed, so that we can prevent executing it multiple times.
        /// </summary>
        private static bool postInstalled;

        /// <summary>
        ///     <see cref="world"/> instance used by our patch and by the loading event handler.
        /// </summary>
        private static World world;

        /// <summary>
        ///     <see cref="prefabSystem"/> instance used by our patch and by the loading event handler. 
        /// </summary>
        private static PrefabSystem prefabSystem;

        /// <summary>
        ///     <para>
        ///         Installing the mode means to add our cloned <see cref="PrefabBase"/> to the global collection in
        ///         <see cref="prefabSystem"/>.
        ///     </para>
        ///     <para>
        ///         To avoid getting the wrong <see cref="world"/> instance we rely on Harmony's <see cref="Traverse"/> to extract the
        ///         <b>m_World</b> field from the injected <see cref="GameManager"/> instance.
        ///     </para>
        ///     <para>
        ///         After that, we leverage <see cref="World.GetOrCreateSystemManaged{T}"/> to get our target <see cref="prefabSystem"/>.
        ///         From there, to get <see cref="prefabSystem"/>'s internal <see cref="PrefabBase"/> list we use <see cref="Traverse"/>
        ///         again and we extract the <b>m_Prefabs</b> field.
        ///     </para>
        ///     <para>
        ///         We now have what it takes to extract our <see cref="PrefabBase"/> object, and as reference we extract the one called
        ///         <b>Grass</b>.
        ///         During this stage we only care for <see cref="ComponentBase"/> and not <see cref="IComponentData"/>.
        ///     </para>
        ///     <para>
        ///         The only <see cref="ComponentBase"/> we need to deal with is the attached <see cref="UIObject"/>, which contains the
        ///         <see cref="UIObject.m_Icon"/> property. This property is a relative URI pointing to a SVG file in your
        ///         <b>Cities2_Data\StreamingAssets\~UI~\GameUI\Media\Game\Icons</b> directory.
        ///     </para>
        ///     <para>
        ///         The <b>Cities2_Data\StreamingAssets\~UI~\GameUI\</b> MUST be omitted from the URI, resulting in a definition similar to:
        ///         <code>
        ///             myUIObject.m_Icon = "Media\Game\Icons\myIcon.svg
        ///         </code>
        ///     </para>
        ///     <para>
        ///         Once the <see cref="UIObject"/> is properly set with an updated <see cref="UIObject.m_Icon"/> and <see cref="UIObject.name"/>
        ///     </para>
        /// </summary>
        internal static void Install()
        {
            var logHeader = $"[{nameof(UpgradesManager)}.{nameof(Install)}]";

            if (installed)
            {
                Log.Debug($"{logHeader} Extended Upgrades is installed, skipping");
                return;
            }

            Log.Debug($"{logHeader} Installing Extended Upgrades");

            world = Traverse.Create(GameManager.instance).Field<World>("m_World").Value;
            if (world is null)
            {
                Log.Error($"{logHeader} Failed retrieving World instance, exiting.");
                return;
            }

            prefabSystem = world.GetExistingSystemManaged<PrefabSystem>();
            if (prefabSystem is null)
            {
                Log.Error($"{logHeader} Failed retrieving PrefabSystem instance, exiting.");
                return;
            }

            var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            if (prefabs is null || !prefabs.Any())
            {
                Log.Error($"{logHeader} Failed retrieving Prefabs list, exiting.");
                return;
            }

            var originalPrefab = prefabs.FirstOrDefault(p => p.name == "Wide Sidewalk");
            if (originalPrefab is null)
            {
                Log.Error($"{logHeader} Failed retrieving the original Grass Prefab instance, exiting.");
                return;
            }

            var originalUIObject = originalPrefab.GetComponent<UIObject>();
            if (originalUIObject is null)
            {
                Log.Error($"{logHeader} Failed retrieving the original Grass Prefab's UIObject instance, exiting.");
                return;
            }
            
            foreach (var upgradeMode in ExtendedRoadUpgrades.Modes)
            {
                if (prefabSystem.TryGetPrefab(new PrefabID(nameof(FencePrefab), upgradeMode.ObsoleteId), out PrefabBase prefabBase))
                {
                    Log.Debug($"{logHeader} [{upgradeMode.ObsoleteId}] Already exists.");
                    return;
                }

                var clonedUIButtonPrefab = Object.Instantiate(originalPrefab);

                clonedUIButtonPrefab.name = upgradeMode.Id;
                
                clonedUIButtonPrefab.Remove<UIObject>();
                clonedUIButtonPrefab.Remove<Unlockable>();
                
                var uiObject = ScriptableObject.CreateInstance<UIObject>();
                uiObject.m_Icon = $"coui://ui-mods/images/ZoneControllerTool.svg";
                uiObject.name = originalUIObject.name.Replace("Grass", upgradeMode.Id);
                uiObject.m_IsDebugObject = originalUIObject.m_IsDebugObject;
                uiObject.m_Priority = originalUIObject.m_Priority-1;
                uiObject.m_Group = originalUIObject.m_Group;
                uiObject.active = originalUIObject.active;
                
                clonedUIButtonPrefab.AddComponentFrom(uiObject);
                var tool = world.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
                
                tool.SetPrefab(clonedUIButtonPrefab);
                
                if (!prefabSystem.AddPrefab(clonedUIButtonPrefab))
                {
                    Log.Error($"{logHeader} [{upgradeMode.Id}] Failed adding the cloned Prefab to PrefabSystem, exiting.");
                    return;
                }
                installed = true;

            }
        }
    }
}
