using CupAPI.Content;
using CupAPI.Utility;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace CupAPI.Patchers {

    [HarmonyPatch(typeof(MapEquipUICardBackSelect))]
    internal static class BackEquipPatcher {

        public static int CurrentPage = 1;

        [HarmonyPatch("ChangeSelection")]
        [HarmonyPrefix]
        private static bool Patch_ChangeSelection(MapEquipUICardBackSelect __instance, Trilean2 direction) {
            int index = TraverseHelper.GetField<int>(__instance, "index");
            MapEquipUICard.Slot slot = TraverseHelper.GetField<MapEquipUICard.Slot>(__instance, "slot");

            if (slot == MapEquipUICard.Slot.CHARM) {
                int lastPage = Registries.CharmPages.Count;
                bool pageChanged = false;

                if (index >= 0 && index <= 4 && direction.y == 1) {
                    if (CurrentPage == 1)
                        CurrentPage = lastPage;
                    else
                        CurrentPage--;
                    pageChanged = true;
                }

                else if (index >= 5 && index <= 8 && direction.y == -1) {
                    if (CurrentPage == lastPage)
                        CurrentPage = 1;
                    else
                        CurrentPage++;
                    pageChanged = true;
                }

                if (pageChanged) {
                    AudioManager.Play("menu_equipment_move");
                    __instance.Setup(slot);
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MapEquipUICardBackSelect))]
    internal static class ContentInjector {

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var instruction in instructions) {
                if (instruction.LoadsField(AccessTools.Field(typeof(MapEquipUICardBackSelect), "CHARMS"))) {
                    yield return CodeInstruction.LoadField(typeof(Registries), "CharmPages")
                        .WithLabels(instruction.labels);
                    yield return CodeInstruction.LoadField(typeof(BackEquipPatcher), "CurrentPage");
                    yield return CodeInstruction.Call(typeof(Dictionary<int, Charm[]>), "get_Item");
                }
                else {
                    yield return instruction;
                }
            }
        }

        private static IEnumerable<MethodBase> TargetMethods() {
            yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Accept");
            yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Unequip");
            yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "Setup");
            yield return AccessTools.Method(typeof(MapEquipUICardBackSelect), "UpdateText");
        }
    }
}