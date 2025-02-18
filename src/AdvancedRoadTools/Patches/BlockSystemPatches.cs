using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Game.Net;
using Game.Prefabs;
using Game.Zones;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace AdvancedRoadTools.Core.Patches;

[HarmonyPatch]
public static class BlockSystemPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlockSystem), "OnUpdate")]
    public static void LogBlockSystemProducer(ref BlockSystem __instance)
    {
        var field = AccessTools.Field(typeof(BlockSystem), "m_UpdatedEdgesQuery");
        var query = (EntityQuery) field.GetValue(__instance);
        var queryCount = query.CalculateEntityCount();
        if (queryCount == 0) return;
        
        AdvancedRoadToolsMod.log.Debug($"Block System producer targets: {queryCount}");
    }

    public partial struct BlockLogJob : IJob
    {
        public void Execute()
        {
            
        }
    }
}