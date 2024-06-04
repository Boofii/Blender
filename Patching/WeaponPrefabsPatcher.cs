using HarmonyLib;
using Blender.Utility;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(LevelPlayerWeaponManager.WeaponPrefabs))]
internal static class WeaponPrefabsPatcher
{

    [HarmonyPatch(nameof(LevelPlayerWeaponManager.WeaponPrefabs.InitWeapon))]
    [HarmonyPrefix]
    private static bool Patch_InitWeapon(LevelPlayerWeaponManager.WeaponPrefabs __instance, Weapon id)
    {
        AbstractLevelWeapon abstractLevelWeapon;
        switch (id)
        {
            default:
                GameObject prefab = ObjectHelper.GetPrefab(id.ToString());
                if (prefab != null)
                {
                    abstractLevelWeapon = prefab.GetComponent<AbstractLevelWeapon>();
                    break;
                }
                return false;
            case Weapon.level_weapon_peashot:
                abstractLevelWeapon = __instance.peashot;
                break;
            case Weapon.level_weapon_spreadshot:
                abstractLevelWeapon = __instance.spread;
                break;
            case Weapon.level_weapon_arc:
                abstractLevelWeapon = __instance.arc;
                break;
            case Weapon.level_weapon_homing:
                abstractLevelWeapon = __instance.homing;
                break;
            case Weapon.level_weapon_exploder:
                abstractLevelWeapon = __instance.exploder;
                break;
            case Weapon.level_weapon_charge:
                abstractLevelWeapon = __instance.charge;
                break;
            case Weapon.level_weapon_boomerang:
                abstractLevelWeapon = __instance.boomerang;
                break;
            case Weapon.level_weapon_bouncer:
                abstractLevelWeapon = __instance.bouncer;
                break;
            case Weapon.level_weapon_wide_shot:
                abstractLevelWeapon = __instance.wideShot;
                break;
            case Weapon.level_weapon_upshot:
                abstractLevelWeapon = __instance.upShot;
                break;
            case Weapon.level_weapon_crackshot:
                abstractLevelWeapon = __instance.crackshot;
                break;
        }

        if (!(abstractLevelWeapon == null))
        {
            AbstractLevelWeapon abstractLevelWeapon2 = UnityEngine.Object.Instantiate(abstractLevelWeapon);
            abstractLevelWeapon2.transform.parent = __instance.root.transform;
            abstractLevelWeapon2.Initialize(__instance.weaponManager, id);
            abstractLevelWeapon2.name = abstractLevelWeapon2.name.Replace("(Clone)", string.Empty);
            __instance.weapons[id] = abstractLevelWeapon2;
        }

        return false;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(WeaponPrefabsPatcher));
    }
}