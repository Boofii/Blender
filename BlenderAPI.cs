using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class BlenderAPI : BaseUnityPlugin
{
    public static bool HasDLC { get; private set; } = false;
    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
    private static new ManualLogSource Logger = null;

    private void Awake()
    {
        base.Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was loaded.");
        BlenderAPI.Logger = base.Logger;

        string dlcBundlePath = Path.Combine(Paths.GameRootPath, "Cuphead_Data\\StreamingAssets\\AssetBundles\\atlas_world_dlc");
        if (File.Exists(dlcBundlePath))
            HasDLC = true;

        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
        EquipMenuPatcher.Initialize(Harmony);
        DetailsPatcher.Initialize(Harmony);
        PrefabsPatcher.Initialize(Harmony);
        LocalizationPatcher.Initialize(Harmony);
        ShopPatcher.Initialize(Harmony);

        AssetHelper.Initialize();
        EquipRegistries.Initialize();
    }

    public static void RegisterLocalization(Identifier id)
    {
        if (!LocalizationPatcher.RegisteredIds.Contains(id))
            LocalizationPatcher.RegisteredIds.Add(id);
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