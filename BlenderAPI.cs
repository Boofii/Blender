using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Utility;
using HarmonyLib;
using System;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class BlenderAPI : BaseUnityPlugin
{

    internal static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
    private static new ManualLogSource Logger = null;

    private void Awake()
    {
        base.Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was initialized.");
        BlenderAPI.Logger = base.Logger;

        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
        EquipMenuPatcher.Initialize(Harmony);
        PropertiesPatcher.Initialize(Harmony);
        WeaponPrefabsPatcher.Initialize(Harmony);
        ObjectHelper.Initialize();

        EquipRegistries.Weapons.Register("level_weapon_pellet",
            (WeaponInfo)new WeaponInfo(typeof(WeaponPellet), typeof(BasicProjectile), "Pellet")
                .SetDisplayName("Pellet")
                .SetSubtext("EX: Big Pellet")
                .SetDescription("Long range with below-average damage.")
                .SetBundleName("blender_content")
                .SetNormalIcons(["pellet0","pellet1","pellet2"])
                .SetGreyIcons(["pellet_grey0","pellet_grey1","pellet_grey2"]));

        CustomData.DataLoadedEvent += delegate
        {
            Weapon pelletWeapon = (Weapon)Enum.Parse(typeof(Weapon), "level_weapon_pellet");
            PlayerData.Data.Gift(PlayerId.PlayerOne, pelletWeapon);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, pelletWeapon);
            PlayerData.SaveCurrentFile();
        };
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