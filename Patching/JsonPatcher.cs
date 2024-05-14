using HarmonyLib;
using System.Collections.Generic;
using System;
using UnityEngine;
using Blender.Utility;

namespace Blender.Patching;

[HarmonyPatch(typeof(JsonUtility))]
internal static class JsonPatcher
{

    private static int saveIndex = 0;

    [HarmonyPatch(nameof(JsonUtility.ToJson), [typeof(object)])]
    [HarmonyPostfix]
    private static void Patch_ToJson(object obj, ref string __result)
    {
        if (obj is PlayerData)
        {
            int currIndex = PlayerData.CurrentSaveFileIndex;
            if (CustomData.Data.TryGetValue(currIndex, out var data))
            {
                try
                {
                    string customJson = SimpleJson.SimpleJson.SerializeObject(data);
                    int entryIndex = __result.Length - 1;
                    __result = __result.Insert(
                        entryIndex,
                        $",\"customData\":{customJson}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to serialize custom data for slot {currIndex} with exception: "
                        + ex);
                }
            }
        }
    }

    [HarmonyPatch(nameof(JsonUtility.FromJson), [typeof(string), typeof(Type)])]
    [HarmonyPostfix]
    private static void Patch_FromJson(string json, ref object __result)
    {
        if (__result is PlayerData)
        {
            string path = "customData";
            if (json.Contains(path))
            {
                if (CustomData.Data.TryGetValue(saveIndex, out var data))
                {
                    int start = json.IndexOf(path) + path.Length + 2;
                    int end = json.Length;
                    string customJson = json.Substring(start, end - start);
                    try
                    {
                        var customData = SimpleJson.SimpleJson.DeserializeObject
                            <Dictionary<string, object>>(customJson);
                        foreach (string key in customData.Keys)
                            data[key] = customData[key];
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to deserialize custom data for slot {saveIndex} with exception: "
                            + ex);
                    }
                }
            }
            saveIndex++;
            if (saveIndex >= 3)
                saveIndex = 0;
        }
    }
}