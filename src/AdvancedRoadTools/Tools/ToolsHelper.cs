using System;
using System.Collections.Generic;
using System.Linq;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using HarmonyLib;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedRoadTools.Tools;

public static class ToolsHelper
{
    public static List<ToolDefinition> ToolDefinitions { get; set; } = new();

    public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
    public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

    public static void RegisterTool(ToolDefinition toolDefinition)
    {
        if (HasTool(toolDefinition))
        {
            log.Error($"Tool \"{toolDefinition.ToolID}\" already registered");
            return;
        }

        log.Info($"Registering tool \"{toolDefinition.ToolID}\" with system \"{toolDefinition.Type.Name}\"");
        ToolDefinitions.Add(toolDefinition);
    }

    public static void CreateToolsUI()
    {
        log.Info($"Creating tools UI. {ToolDefinitions.Count} registered tools");
        var world = Traverse.Create(GameManager.instance).Field<World>("m_World").Value;
        var prefabSystem = world.GetExistingSystemManaged<PrefabSystem>();
        var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        var originalPrefab = prefabs.FirstOrDefault(p => p.name == "Wide Sidewalk");
        var originalUIObject = originalPrefab?.GetComponent<UIObject>();

        foreach (var definition in ToolDefinitions)
        {
            try
            {
                var clonedUIButtonPrefab = Object.Instantiate(originalPrefab);

                clonedUIButtonPrefab.name = definition.ToolID;

                clonedUIButtonPrefab.Remove<UIObject>();
                clonedUIButtonPrefab.Remove<Unlockable>();

                var uiObject = ScriptableObject.CreateInstance<UIObject>();
                uiObject.m_Icon = definition.ui.ImagePath;
                uiObject.name = definition.ToolID;
                //uiObject.m_IsDebugObject = originalUIObject.m_IsDebugObject;
                uiObject.m_Priority = definition.Priority;
                uiObject.m_Group = originalUIObject.m_Group;
                uiObject.active = originalUIObject.active;

                clonedUIButtonPrefab.AddComponentFrom(uiObject);

                var tool = world.GetOrCreateSystemManaged(definition.Type) as IARTTool;

                tool.SetPrefab(clonedUIButtonPrefab);
                
                if (!prefabSystem.AddPrefab(clonedUIButtonPrefab))
                {
                    log.Error($"Tool \"{definition.ToolID}\" could not be added to \"PrefabSystem\"");
                    continue;
                }
                log.Info($"\tTool \"{definition.ToolID}\" was successfully created");
            }
            catch (Exception e)
            {
                log.Error($"\tTool \"{definition.ToolID}\" could not be created: {e}");
            }
        }
    }
}