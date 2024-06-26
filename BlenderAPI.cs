﻿using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Testing;
using Blender.Utility;
using HarmonyLib;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class BlenderAPI : BaseUnityPlugin
{
    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
    private static new ManualLogSource Logger = null;

    private void Awake()
    {
        base.Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was loaded.");
        BlenderAPI.Logger = base.Logger;

        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
        EquipMenuPatcher.Initialize(Harmony);
        IconPatcher.Initialize(Harmony);
        PrefabsPatcher.Initialize(Harmony);
        LocalizationPatcher.Initialize(Harmony);
        EquipRegistries.Initialize();
        AssetHelper.Initialize();

        Tester.Initialize(Harmony);
    }

    internal static void LogInfo(string message)
    {
        Logger.LogInfo(message);
    }

    internal static void LogWarning(string message)
    {
        Logger.LogWarning(message);
    }

    internal static void LogError(string message)
    {
        Logger.LogError(message);
    }
}