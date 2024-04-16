using CupAPI.Content;
using CupAPI.Utility;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace CupAPI.Patching {
    [HarmonyPatch(typeof(WeaponProperties))]
    internal static class PropertiesPatcher {

        [HarmonyPatch(nameof(WeaponProperties.GetDisplayName), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetDisplayName(Charm charm, ref string __result) {
            IEquipInfo charmInfo = ContentManager.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.DisplayName;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetSubtext), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetSubtext(Charm charm, ref string __result) {
            IEquipInfo charmInfo = ContentManager.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.Subtext;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetDescription), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetDescription(Charm charm, ref string __result) {
            IEquipInfo charmInfo = ContentManager.Charms.Get(charm.ToString());
            if (charmInfo != null) {
                __result = charmInfo.Description;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(WeaponProperties.GetIconPath), [typeof(Charm)])]
        [HarmonyPrefix]
        private static bool Patch_GetIconPath(Charm charm, ref string __result) {
            if (ContentManager.Charms.ContainsName(charm.ToString())) {
                __result = charm.ToString();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AbstractMapCardIcon), "setIcons")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Patch_setIcons(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            int firstBltIndex = list.FindIndex(inst => inst.opcode == OpCodes.Blt);
            int lastBltIndex = list.FindLastIndex(inst => inst.opcode == OpCodes.Blt);

            for (int i = 0; i < list.Count; i++) {
                yield return list[i];
                if (i == firstBltIndex || i == lastBltIndex) {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    if (i == firstBltIndex)
                        yield return CodeInstruction.Call(typeof(PropertiesPatcher), nameof(AddNormalSprites));
                    else if (i == lastBltIndex)
                        yield return CodeInstruction.Call(typeof(PropertiesPatcher), nameof(AddGreySprites));
                }
            }
        }

        private static void AddNormalSprites(string iconPath, List<Sprite> list) {
            if (ContentManager.Charms.ContainsName(iconPath)) {
                IEquipInfo charmInfo = ContentManager.Charms.Get(iconPath);
                foreach (string icon in charmInfo.NormalIcons) {
                    Sprite sprite = AssetHelper.CacheAsset<Sprite>(charmInfo.BundleName, icon);
                    if (sprite != null)
                        list.Add(sprite);
                }
            }
        }

        private static void AddGreySprites(string iconPath, List<Sprite> list) {
            if (ContentManager.Charms.ContainsName(iconPath)) {
                IEquipInfo charmInfo = ContentManager.Charms.Get(iconPath);
                foreach (string icon in charmInfo.GreyIcons) {
                    Sprite sprite = AssetHelper.CacheAsset<Sprite>(charmInfo.BundleName, icon);
                    if (sprite != null)
                        list.Add(sprite);
                }
            }
        }
    }
}