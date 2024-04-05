using CupAPI.Utility;
using HarmonyLib;
using MonoMod.Utils;
namespace CupAPI.Patchers {
    [HarmonyPatch(typeof(MapEquipUICardBackSelect))]
    internal static class BackEquipPatch {

        [HarmonyPatch(nameof(MapEquipUICardBackSelect.Setup))]
        [HarmonyPrefix]
        private static void Patch_Setup(MapEquipUICardBackSelect __instance, MapEquipUICard.Slot slot) {

        }
    }
}