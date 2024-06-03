using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Utility;
using CupAPI.Patching;
using HarmonyLib;

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

        EquipRegistries.Charms.Register("charm_float", new EquipInfo()
            .SetDisplayName("Float")
            .SetSubtext("Fall Slowly")
            .SetDescription("Allows you to fall slowly when holding the jump button.")
            .SetBundleName("blender_content")
            .SetNormalIcons(["float0", "float1", "float2"]));

        /*EquipRegistries.Weapons.Register("level_weapon_pellet",
            (WeaponInfo)new WeaponInfo(typeof(WeaponPellet), typeof(BasicProjectile), "Pellet")
                .SetDisplayName("Pellet")
                .SetSubtext("EX: Big Pellet")
                .SetDescription("Long range with below-average damage.")
                .SetBundleName("blender_content")
                .SetNormalIcons(["pellet0","pellet1","pellet2"])
                .SetGreyIcons(["pellet_grey0","pellet_grey1","pellet_grey2"]));

        CustomData.DataLoadedEvent += delegate
        {
            Charm floatCharm = Charm.charm_float;
            Weapon pelletWeapon = (Weapon)Enum.Parse(typeof(Weapon), "level_weapon_pellet");
            PlayerData.Data.Gift(PlayerId.PlayerOne, floatCharm);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, floatCharm);
            PlayerData.Data.Gift(PlayerId.PlayerOne, pelletWeapon);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, pelletWeapon);
            PlayerData.SaveCurrentFile();
        };*/

        /*Harmony.PatchAll(typeof(Initializer));
        EnumRegistry<Weapon> registry = EnumManager.Register<Weapon>();
        registry.Register("level_weapon_pellet");
        Logger.LogInfo(string.Join(",", Enum.GetNames(typeof(Weapon))));
        Pellet = (Weapon)Enum.Parse(typeof(Weapon), "level_weapon_pellet");

        CustomData.DataLoadedEvent += delegate
        {
            PlayerData.Data.Gift(PlayerId.PlayerOne, Pellet);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, Pellet);
            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).primaryWeapon = Pellet;
            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).primaryWeapon = Pellet;
            PlayerData.SaveCurrentFile();
        };*/
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

    /*private static AbstractLevelWeapon GetWeapon(Weapon id)
    {
        if (id != Pellet)
            return null;
        if (AssetHelper.TryGetAsset("blender_core", "Empty", out GameObject empty))
        {
            if (empty.GetComponents(typeof(WeaponPellet)).Count() == 0)
                empty.AddComponent<WeaponPellet>();
            WeaponPellet weapon = empty.GetComponent<WeaponPellet>();

            if (AssetHelper.TryGetAsset("blender_core", "Pellet", out GameObject pellet))
            {
                if (pellet.GetComponents(typeof(BasicProjectile)).Count() == 0)
                    pellet.AddComponent<BasicProjectile>();
                BasicProjectile projectile = pellet.GetComponent<BasicProjectile>();

                SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
                renderer.sortingLayerName = "Projectiles";
                print(renderer.sprite.name);
                print(renderer.sprite.uv);

                Traverse traverse = Traverse.Create(weapon);
                traverse.Field<AbstractProjectile>("basicPrefab").Value = projectile;
            }
            return weapon;
        }
        return null;
    }

    [HarmonyPatch(typeof(LevelPlayerWeaponManager.WeaponPrefabs), "InitWeapon")]
    [HarmonyPrefix]
    private static bool Patch_InitWeapon(LevelPlayerWeaponManager.WeaponPrefabs __instance, Weapon id)
    {
        AbstractLevelWeapon abstractLevelWeapon = null;

        if (id == Pellet)
        {
            abstractLevelWeapon = GetWeapon(id);
        }

        Traverse traverse = Traverse.Create(__instance);
        switch (id)
        {
            case Weapon.level_weapon_peashot:
                abstractLevelWeapon = traverse.Field("peashot").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_spreadshot:
                abstractLevelWeapon = traverse.Field("spread").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_arc:
                abstractLevelWeapon = traverse.Field("arc").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_homing:
                abstractLevelWeapon = traverse.Field("homing").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_exploder:
                abstractLevelWeapon = traverse.Field("exploder").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_charge:
                abstractLevelWeapon = traverse.Field("charge").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_boomerang:
                abstractLevelWeapon = traverse.Field("boomerang").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_bouncer:
                abstractLevelWeapon = traverse.Field("bouncer").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_wide_shot:
                abstractLevelWeapon = traverse.Field("wideShot").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_upshot:
                abstractLevelWeapon = traverse.Field("upShot").GetValue<AbstractLevelWeapon>();
                break;
            case Weapon.level_weapon_crackshot:
                abstractLevelWeapon = traverse.Field("crackshot").GetValue<AbstractLevelWeapon>();
                break;
        }

        if (!(abstractLevelWeapon == null))
        {
            AbstractLevelWeapon abstractLevelWeapon2 = UnityEngine.Object.Instantiate(abstractLevelWeapon);
            abstractLevelWeapon2.transform.parent = traverse.Field("root").GetValue<Transform>().transform;
            abstractLevelWeapon2.Initialize(traverse.Field("weaponManager").GetValue<LevelPlayerWeaponManager>(), id);
            abstractLevelWeapon2.name = abstractLevelWeapon2.name.Replace("(Clone)", string.Empty);
            traverse.Field("weapons").GetValue<Dictionary<Weapon, AbstractLevelWeapon>>()[id] = abstractLevelWeapon2;
        }

        return false;
    }*/
}