using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CupAPI.Utility {
    public static class AssetCache {

        private static readonly Dictionary<string, object> Assets = new Dictionary<string, object>();

        public static void AddAsset<T>(string path, T asset) where T : Object {
            if (!Assets.ContainsKey(path))
                Assets[path] = asset;
        }

        public static bool GetAsset<T>(string path, out T asset) where T : Object {
            asset = default;
            if (Assets.TryGetValue(path, out var asst) && asst is T) {
                asset = (T)asst;
                return true;
            }

            return false;
        }

        public static bool HasAsset(string path) {
            return Assets.ContainsKey(path);
        }

        public static List<object> GetAssets() {
            return Assets.Values.ToList();
        }
    }
}