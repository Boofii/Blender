using Blender.Content;
using Blender.Utility;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(WeaponProperties))]
internal static class DetailsPatcher
{
    [HarmonyPatch(nameof(WeaponProperties.GetValue), [typeof(Charm)])]
    [HarmonyPrefix]
    private static bool Patch_GetCharmCost(Charm charm, ref int __result)
    {
        if (EquipRegistries.Charms.ContainsName(charm.ToString()))
        {
            EquipInfo info = EquipRegistries.Charms.GetValue(charm.ToString());
            __result = info.Cost;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(WeaponProperties.GetValue), [typeof(Weapon)])]
    [HarmonyPrefix]
    private static bool Patch_GetWeaponCost(Weapon weapon, ref int __result)
    {
        if (EquipRegistries.Weapons.ContainsName(weapon.ToString()))
        {
            EquipInfo info = EquipRegistries.Weapons.GetValue(weapon.ToString());
            __result = info.Cost;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(WeaponProperties.GetIconPath), [typeof(Charm)])]
    [HarmonyPrefix]
    private static bool Patch_GetCharmIcon(Charm charm, ref string __result)
    {
        if (EquipRegistries.Charms.ContainsName(charm.ToString()))
        {
            __result = charm.ToString();
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(WeaponProperties.GetIconPath), [typeof(Weapon)])]
    [HarmonyPrefix]
    private static bool Patch_GetWeaponIcon(Weapon weapon, ref string __result)
    {
        if (EquipRegistries.Weapons.ContainsName(weapon.ToString()))
        {
            __result = weapon.ToString();
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(AbstractMapCardIcon), "setIcons")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Patch_setIcons(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> list = new List<CodeInstruction>(instructions);
        int firstBltIndex = list.FindIndex(inst => inst.opcode == OpCodes.Blt);
        int lastBltIndex = list.FindLastIndex(inst => inst.opcode == OpCodes.Blt);

        for (int i = 0; i < list.Count; i++)
        {
            yield return list[i];
            if (i == firstBltIndex || i == lastBltIndex)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                if (i == firstBltIndex)
                    yield return CodeInstruction.Call(typeof(DetailsPatcher), nameof(AddNormalSprites));
                else if (i == lastBltIndex)
                    yield return CodeInstruction.Call(typeof(DetailsPatcher), nameof(AddGreySprites));
            }
        }
    }

    private static void AddNormalSprites(string iconPath, List<Sprite> list)
    {
        if (EquipRegistries.Charms.ContainsName(iconPath))
        {
            EquipInfo info = EquipRegistries.Charms.GetValue(iconPath);
            foreach (string icon in info.NormalIcons)
               AssetHelper.LoadAsset<Sprite>(info.BundleId, icon, list.Add);
        }
        else if (EquipRegistries.Weapons.ContainsName(iconPath))
        {
            EquipInfo info = EquipRegistries.Weapons.GetValue(iconPath);
            foreach (string icon in info.NormalIcons)
                AssetHelper.LoadAsset<Sprite>(info.BundleId, icon, list.Add);
        }
    }

    private static void AddGreySprites(string iconPath, List<Sprite> list)
    {
        if (EquipRegistries.Weapons.ContainsName(iconPath))
        {
            EquipInfo info = EquipRegistries.Weapons.GetValue(iconPath);
            foreach (string icon in info.GreyIcons)
                AssetHelper.LoadAsset<Sprite>(info.BundleId, icon, list.Add);
        }
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(DetailsPatcher));
    }
}