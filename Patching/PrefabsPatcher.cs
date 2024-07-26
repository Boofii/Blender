using HarmonyLib;
using Blender.Utility;
using UnityEngine;
using Blender.Content;

namespace Blender.Patching;

internal static class PrefabsPatcher
{
    [HarmonyPatch(typeof(LevelPlayerWeaponManager.WeaponPrefabs), nameof(LevelPlayerWeaponManager.WeaponPrefabs.InitWeapon))]
    [HarmonyPrefix]
    private static bool Patch_InitWeapon(LevelPlayerWeaponManager.WeaponPrefabs __instance, Weapon id)
    {
        if (EquipRegistries.Weapons.ContainsName(id.ToString()))
        {
            GameObject prefab = AssetHelper.GetPrefab(id.ToString());
            if (prefab != null)
            {
                AbstractLevelWeapon weaponComponent = prefab.GetComponent<AbstractLevelWeapon>();
                AbstractLevelWeapon weaponClone = UnityEngine.Object.Instantiate(weaponComponent);
                weaponClone.transform.parent = __instance.root.transform;
                weaponClone.Initialize(__instance.weaponManager, id);
                weaponClone.name = weaponClone.name.Replace("(Clone)", string.Empty);
                __instance.weapons[id] = weaponClone;
            }
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(LevelPlayerWeaponManager.SuperPrefabs), nameof(LevelPlayerWeaponManager.SuperPrefabs.GetPrefab))]
    [HarmonyPrefix]
    private static bool Patch_GetSuper(Super super, ref AbstractPlayerSuper __result)
    {
        if (EquipRegistries.Supers.ContainsName(super.ToString()))
        {
            GameObject prefab = AssetHelper.GetPrefab(super.ToString());
            if (prefab != null)
            {
                AbstractPlayerSuper superComponent = prefab.GetComponent<AbstractPlayerSuper>();
                __result = superComponent;
            }
            return false;
        }
        return true;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(PrefabsPatcher));
    }
}