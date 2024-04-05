using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupAPI.Util {
    [HarmonyPatch(typeof(JsonUtility))]
    public static class CustomData {

        internal static readonly Dictionary<int, Dictionary<string, object>> Data = [];
        public static Action DataLoadedEvent;

        public static bool TryGet<T>(int slot, string key, out T value) where T : class {
            value = null;
            if (Data.TryGetValue(slot, out var customData)) {
                if (customData.TryGetValue(key, out var val))
                    try {
                        value = SimpleJson.SimpleJson.DeserializeObject<T>(val.ToString());
                        if (value != null)
                            return true;
                    }
                    catch (Exception) {}
            }
            return false;
        }

        public static object Get(int slot, string key) {
            if (Data.TryGetValue(slot, out var customData)) {
                if (customData.TryGetValue(key, out var value))
                    return value;
            }
            return null;
        }

        public static void Set(int slot, string key, object value) {
            if (Data.TryGetValue(slot, out var customData))
                customData[key] = value;
        }

        public static void Remove(int slot, string key) {
            if (Data.TryGetValue(slot, out var customData)) {
                if (customData.ContainsKey(key))
                    customData.Remove(key);
            }
        }

        internal static void Initialize() {
            for (int i = 0; i < 3; i++)
                Data[i] = [];
        }

        [HarmonyPatch(typeof(PlayerData), "OnLoaded")]
        [HarmonyPostfix]
        private static void Patch_OnLoaded() {
            DataLoadedEvent?.Invoke();
        }
    }
}