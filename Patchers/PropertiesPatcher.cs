using CupAPI.Content;
using CupAPI.Utility;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace CupAPI.Patchers {
    [HarmonyPatch(typeof(WeaponProperties))]
    internal static class PropertiesPatcher {

        [HarmonyPatch(nameof(WeaponProperties.GetDisplayName), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetDisplayName(Charm charm, ref string __result) {
            ICharm charmInfo = Registries.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.DisplayName;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetSubtext), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetSubtext(Charm charm, ref string __result) {
            ICharm charmInfo = Registries.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.Subtext;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetDescription), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetDescription(Charm charm, ref string __result) {
            ICharm charmInfo = Registries.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.Description;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetIconPath), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetIconPath(Charm charm, ref string __result) {
            ICharm charmInfo = Registries.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = $"{charmInfo.IconBundle}:{charmInfo.IconName}";
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AbstractMapCardIcon), "setIcons")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Patch_SetIcons(IEnumerable<CodeInstruction> instructions) {
            bool found = false;
            
            foreach (var instruction in instructions) {
                yield return instruction;
                if (instruction.opcode == OpCodes.Blt && !found) {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return CodeInstruction.Call(typeof(PropertiesPatcher), nameof(AddCustomSprites));
                    found = true;
                }
            }
        }

        private static void AddCustomSprites(string iconPath, List<Sprite> list) {
            if (AssetCache.GetAsset(iconPath, out Sprite sprite))
                list.Add(sprite);
        }
    }
}