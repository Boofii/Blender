using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CupAPI.Util {
    public static class AssetHelper {

        private static readonly Dictionary<string, AssetBundle> bundles = [];

        public static AssetBundle LoadBundle(string bundleName) {
            string modDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string assetsPath = Path.Combine(modDir, "Assets");
            string bundlePath = Path.Combine(assetsPath, bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null) {
                CupAPI.Logger.LogError($"Couldn't find an asset bundle with path {bundlePath}");
                return null;
            }
            bundles.Add(bundleName, bundle);
            return bundle;
        }

        public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects) {
            AssetBundle bundle = bundles[bundleName];
            if (bundle == null) {
                CupAPI.Logger.LogError($"Couldn't find an asset bundle with name {bundleName}");
                return;
            }
            bundle.Unload(unloadAllLoadedObjects);
        }

        public static List<string> GetBundleNames() {
            return bundles.Keys.ToList();
        }
    }
}