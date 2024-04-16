using CupAPI.Content;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace CupAPI.Patching {

    [HarmonyPatch(typeof(MapEquipUICardBackSelect))]
    internal static class EquipMenuPatcher {

        public static int CurrentPage1 = 1;

        [HarmonyPatch(nameof(MapEquipUICardBackSelect.ChangeSelection))]
        [HarmonyPrefix]
        private static bool Patch_ChangeSelection(MapEquipUICardBackSelect __instance, Trilean2 direction, MapEquipUICard.Slot ___slot, int ___index) {
            switch (___slot) {
                case MapEquipUICard.Slot.CHARM:
                    bool pageChanged = false;
                    int pageCount = ContentManager.CharmPages.Count;

                    if (pageCount == 1)
                        return true;

                    if (___index >= 0 && ___index <= 4 && direction.y == 1) {
                        if (CurrentPage1 == 1)
                            CurrentPage1 = pageCount;
                        else
                            CurrentPage1--;
                        pageChanged = true;
                    }

                    else if (___index >= 5 && ___index <= 8 && direction.y == -1) {
                        if (CurrentPage1 == pageCount)
                            CurrentPage1 = 1;
                        else
                            CurrentPage1++;
                        pageChanged = true;
                    }

                    if (pageChanged) {
                        AudioManager.Play("menu_equipment_move");
                        __instance.Setup(___slot);
                        return false;
                    }
                    return true;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapEquipUICardBackSelect))]
    internal static class ContentAdder {

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var instruction in instructions) {
                if (instruction.LoadsField(AccessTools.Field(typeof(MapEquipUICardBackSelect), "CHARMS"))) {
                    yield return CodeInstruction.LoadField(typeof(ContentManager), nameof(ContentManager.CharmPages))
                        .WithLabels(instruction.labels);
                    yield return CodeInstruction.LoadField(typeof(EquipMenuPatcher), nameof(EquipMenuPatcher.CurrentPage1));
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