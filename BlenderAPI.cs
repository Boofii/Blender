using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Utility;
using HarmonyLib;
using System.IO;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class BlenderAPI : BaseUnityPlugin
{
    public static bool HasDLC { get; private set; } = false;
    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
    private static new ManualLogSource Logger = null;

    private void Awake() {
        base.Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was loaded.");
        BlenderAPI.Logger = base.Logger;

        string dlcBundlePath = Path.Combine(Paths.GameRootPath, "Cuphead_Data\\StreamingAssets\\AssetBundles\\atlas_world_dlc");
        if (File.Exists(dlcBundlePath))
            HasDLC = true;

        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
        EquipMenuPatcher.Initialize(Harmony);
        PropertiesPatcher.Initialize(Harmony);
        PrefabsPatcher.Initialize(Harmony);
        LocalizationPatcher.Initialize(Harmony);
        ShopPatcher.Initialize(Harmony);
        AudioPatcher.Initialize(Harmony);
        AssetHelper.Initialize(Harmony);
        ScenePatcher.Initialize(Harmony);
        EquipRegistries.Initialize();
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

    public static readonly Weapon[] OriginalWeapons = {
        Weapon.level_weapon_peashot,
        Weapon.level_weapon_spreadshot,
        Weapon.level_weapon_arc,
        Weapon.level_weapon_homing,
        Weapon.level_weapon_exploder,
        Weapon.level_weapon_boomerang,
        Weapon.level_weapon_charge,
        Weapon.level_weapon_bouncer,
        Weapon.level_weapon_wide_shot,
        Weapon.level_weapon_accuracy,
        Weapon.level_weapon_firecracker,
        Weapon.level_weapon_upshot,
        Weapon.level_weapon_firecrackerB,
        Weapon.level_weapon_pushback,
        Weapon.plane_weapon_peashot,
        Weapon.plane_weapon_laser,
        Weapon.plane_weapon_bomb,
        Weapon.plane_chalice_weapon_3way,
        Weapon.arcade_weapon_peashot,
        Weapon.arcade_weapon_rocket_peashot,
        Weapon.plane_chalice_weapon_bomb,
        Weapon.level_weapon_crackshot,
        Weapon.level_weapon_splitter
    };
}