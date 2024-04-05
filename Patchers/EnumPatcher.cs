using CupAPI.Utility;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CupAPI.Patchers {
    [HarmonyPatch(typeof(Enum))]
    internal static class EnumPatcher {

        [HarmonyPatch(nameof(Enum.Parse), [typeof(Type), typeof(string), typeof(bool)])]
        [HarmonyPrefix]
        private static bool Patch_Parse(Type enumType, string value, bool ignoreCase, ref object __result) {
            if (EnumManager.TryGetRegistry(enumType, out var registry)) {
                int intValue = registry.GetId(value);
                if (intValue != 0) {
                    __result = Enum.ToObject(enumType, intValue);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Enum.IsDefined))]
        [HarmonyPrefix]
        private static bool Patch_IsDefined(Type enumType, object value, ref bool __result) {
            if (EnumManager.TryGetRegistry(enumType, out var registry)) {
                if (value.GetType() == typeof(int) && registry.ContainsId((int)value)) {
                    __result = true;
                    return false;
                }
                else if (value.GetType() == typeof(string) && registry.ContainsName((string)value)) {
                    __result = true;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Enum.ToString), new Type[] { })]
        [HarmonyPrefix]
        private static bool Patch_ToString(Enum __instance, ref string __result) {
            if (EnumManager.TryGetRegistry(__instance.GetType(), out var registry)) {
                string name = registry.GetName(Convert.ToInt32(__instance));
                if (name != null) {
                    __result = name;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Enum.GetNames))]
        [HarmonyPostfix]
        private static void Patch_GetNames(Type enumType, ref string[] __result) {
            if (EnumManager.TryGetRegistry(enumType, out var registry)) {
                List<string> newNames = registry.GetNames();
                List<string> list = [];

                foreach (string name in __result)
                    list.Add(name);
                foreach (string name in newNames)
                    list.Add(name);

                __result = list.ToArray();
            }
        }

        [HarmonyPatch(nameof(Enum.GetValues))]
        [HarmonyPostfix]
        private static void Patch_GetValues(Type enumType, ref Array __result) {
            if (EnumManager.TryGetRegistry(enumType, out var registry)) {
                List<int> newValues = registry.GetIds();
                int newCount = __result.Length + newValues.Count;
                Array array = Array.CreateInstance(enumType, newCount);

                int i = 0;
                foreach (var value in __result) {
                    array.SetValue(value, i);
                    i++;
                }
                foreach (var value in newValues) {
                    array.SetValue(value, i);
                    i++;
                }

                __result = array;
            }
        }

        [HarmonyPatch(nameof(Enum.GetName))]
        [HarmonyPrefix]
        private static bool Patch_GetName(Type enumType, object value, ref string __result) {
            if (EnumManager.TryGetRegistry(enumType, out var registry)) {
                int intValue = (int) value;
                string name = registry.GetName(intValue);
                if (name != null) {
                    __result = name;
                    return false;
                }
            }
            return true;
        }
    }
}