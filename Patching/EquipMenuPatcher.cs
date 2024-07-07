using Blender.Content;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(MapEquipUICardBackSelect))]
internal static class EquipMenuPatcher
{
    internal static int[] CharmsPage = [1, 1];
    internal static int[] ShotAPage = [1, 1];
    internal static int[] ShotBPage = [1, 1];

    [HarmonyPatch(nameof(MapEquipUICardBackSelect.ChangeSelection))]
    [HarmonyPrefix]
    private static bool Patch_ChangeSelection(MapEquipUICardBackSelect __instance, Trilean2 direction)
    {
        switch (__instance.slot)
        {
            case MapEquipUICard.Slot.CHARM:
                return ChangePages(__instance, direction, EquipRegistries.CharmPages, ref CharmsPage, (charm, id) => PlayerData.Data.IsUnlocked((PlayerId)id, charm));
            case MapEquipUICard.Slot.SHOT_A:
                return ChangePages(__instance, direction, EquipRegistries.WeaponPages, ref ShotAPage, (weapon, id) => PlayerData.Data.IsUnlocked((PlayerId)id, weapon));
            case MapEquipUICard.Slot.SHOT_B:
                return ChangePages(__instance, direction, EquipRegistries.WeaponPages, ref ShotBPage, (weapon, id) => PlayerData.Data.IsUnlocked((PlayerId)id, weapon));
            case MapEquipUICard.Slot.SUPER:
                break;
        }
        return true;
    }

    private static bool ChangePages<TEnum>(MapEquipUICardBackSelect instance, Trilean2 direction, Dictionary<int, TEnum[]> pagesDict, ref int[] pagesArray, Func<TEnum, int, bool> unlockedPredicate) where TEnum : Enum
    {
        int id = (int)instance.playerID;
        int index = instance.index;
        bool pageChanged = false;
        int pageCount = pagesDict.Count;

        if (pageCount == 1)
            return true;

        int unlockedCount = 0;
        for (int i = 2; i <= pageCount; i++)
            if (pagesDict.TryGetValue(i, out TEnum[] values))
                unlockedCount += values.Where(unlockedPredicate.Invoke).Count();
        if (unlockedCount == 0)
            return true;

        int switchIndex = BlenderAPI.HasDLC ? 5 : 3;
        if (index >= 0 && index <= switchIndex - 1 && direction.y == 1 && pagesArray[id] != 1)
        {
            pagesArray[id]--;
            pageChanged = true;
        }
        else if (index >= switchIndex && direction.y == -1 && pagesArray[id] != pageCount)
        {
            pagesArray[id]++;
            pageChanged = true;
        }

        if (pageChanged)
        {
            AudioManager.Play("menu_equipment_move");
            instance.Setup(instance.slot);
            return false;
        }
        return true;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(EquipMenuPatcher));
        harmony.PatchAll(typeof(ContentAdder));
    }
}

[HarmonyPatch(typeof(MapEquipUICardBackSelect))]
internal static class ContentAdder
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(AccessTools.Field(typeof(MapEquipUICardBackSelect), "CHARMS")))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(instruction.labels);
                yield return CodeInstruction.Call(typeof(ContentAdder), nameof(ContentAdder.GetCharms));
            }
            else if (instruction.LoadsField(AccessTools.Field(typeof(MapEquipUICardBackSelect), "WEAPONS"))) {
                yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(instruction.labels);
                yield return CodeInstruction.Call(typeof(ContentAdder), nameof(ContentAdder.GetWeapons));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static Charm[] GetCharms(MapEquipUICardBackSelect instance)
    {
        int id = (int)instance.playerID;
        return EquipRegistries.CharmPages[EquipMenuPatcher.CharmsPage[id]];
    }

    private static Weapon[] GetWeapons(MapEquipUICardBackSelect instance)
    {
        int id = (int)instance.playerID;
        int page = instance.slot == MapEquipUICard.Slot.SHOT_A ?
            EquipMenuPatcher.ShotAPage[id] :
            EquipMenuPatcher.ShotBPage[id];

        return EquipRegistries.WeaponPages[page];
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Accept");
        yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Unequip");
        yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Setup");
        yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "UpdateText");
    }
}