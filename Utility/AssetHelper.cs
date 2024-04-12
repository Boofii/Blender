using CupAPI.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CupAPI.Util {
    public static class AssetHelper {

        private static readonly Dictionary<string, AssetBundle> Bundles = [];

        public static AssetBundle LoadBundle(string bundleName) {
            if (!Bundles.ContainsKey(bundleName)) {
                string modDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                string assetsPath = Path.Combine(modDir, "Assets");
                string bundlePath = Path.Combine(assetsPath, bundleName);
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null) {
                    CupAPI.Logger.LogError($"Couldn't find an asset bundle with path {bundlePath}");
                    return null;
                }
                Bundles.Add(bundleName, bundle);
                return bundle;
            }
            return Bundles[bundleName];
        }

        public static bool UnloadBundle(string bundleName, bool unloadAllLoadedObjects) {
            if (Bundles.TryGetValue(bundleName, out var bundle)) {
                Bundles.Remove(bundleName);
                bundle.Unload(unloadAllLoadedObjects);
                return true;
            }

            CupAPI.Logger.LogError($"Couldn't find an asset bundle with name {bundleName}");
            return false;
        }

        public static bool TryGetBundle(string bundleName, out AssetBundle bundle) {
            bundle = null;
            if (Bundles.TryGetValue(bundleName, out var bnd)) {
                bundle = bnd;
                return true;
            }

            CupAPI.Logger.LogError($"Couldn't find an asset bundle with name {bundleName}");
            return false;
        }

        public static void CacheAssets<T>(string bundleName) where T : Object {
            if (!HasBundle(bundleName))
                LoadBundle(bundleName);

            if (TryGetBundle(bundleName, out AssetBundle bundle)) {
                T[] assets = bundle.LoadAllAssets<T>();
                foreach (T asset in assets)
                    AssetCache.AddAsset($"{bundleName}:{asset.name}", asset);
                UnloadBundle(bundleName, false);
            }
        }

        public static void CacheAsset<T>(string bundleName, string assetName) where T : Object {
            if (!HasBundle(bundleName))
                LoadBundle(bundleName);

            if (TryGetBundle(bundleName, out AssetBundle bundle)) {
                AssetBundleRequest assetRequest = bundle.LoadAssetAsync<T>(assetName);
                assetRequest.completed += delegate (AsyncOperation operation) {
                    AssetCache.AddAsset($"{bundleName}:{assetRequest.asset.name}", assetRequest.asset);
                    UnloadBundle(bundleName, false);
                };
            }
        }

        public static bool HasBundle(string bundleName) {
            return Bundles.ContainsKey(bundleName);
        }

        public static List<AssetBundle> GetBundles() {
            return Bundles.Values.ToList();
        }
    }
}