using Blender.Patching;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Blender.Utility;

public static class CustomData
{

    internal static readonly Dictionary<int, Dictionary<string, object>> Data = [];
    public static Action DataLoadedEvent { get; set; }

    public static object Get(int slot, string key)
    {
        if (Data.TryGetValue(slot, out var customData))
        {
            if (customData.TryGetValue(key, out var value))
                return value;
        }
        return null;
    }

    public static void Set(int slot, string key, object value)
    {
        if (Data.TryGetValue(slot, out var customData))
            customData[key] = value;
    }

    public static void Remove(int slot, string key)
    {
        if (Data.TryGetValue(slot, out var customData))
        {
            if (customData.ContainsKey(key))
                customData.Remove(key);
        }
    }

    internal static void Initialize(Harmony harmony)
    {
        for (int i = 0; i < 3; i++)
            Data[i] = [];
        harmony.PatchAll(typeof(CustomData));
        harmony.PatchAll(typeof(JsonPatcher));
    }

    [HarmonyPatch(typeof(PlayerData), "OnLoaded")]
    [HarmonyPostfix]
    private static void Patch_OnLoaded()
    {
        DataLoadedEvent?.Invoke();
    }
}