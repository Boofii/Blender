using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Blender.Utility;

public static class AssetHelper
{

    private static readonly Dictionary<string, AssetBundle> Bundles = [];
    private static readonly Dictionary<string, Object> Assets = [];

    public static AssetBundle LoadBundle(string modName, string bundleName)
    {
        if (!Bundles.ContainsKey(bundleName))
        {
            string modDir = Path.Combine(Paths.PluginPath, modName);
            string assetsPath = Path.Combine(modDir, "Assets");
            string bundlePath = Path.Combine(assetsPath, bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                BlenderAPI.LogWarning($"Couldn't find an asset bundle with path {bundlePath}.");
                return null;
            }
            Bundles.Add(bundleName, bundle);
            return bundle;
        }
        return Bundles[bundleName];
    }

    public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects)
    {
        if (Bundles.TryGetValue(bundleName, out var bundle))
        {
            bundle.Unload(unloadAllLoadedObjects);
            Bundles.Remove(bundleName);
            return;
        }

        BlenderAPI.LogWarning($"Couldn't find an asset bundle with name {bundleName}.");
    }

    public static bool TryGetBundle(string bundleName, out AssetBundle bundle)
    {
        bundle = null;
        if (Bundles.TryGetValue(bundleName, out var bnd))
        {
            bundle = bnd;
            return true;
        }

        BlenderAPI.LogWarning($"Couldn't find an asset bundle with name {bundleName}.");
        return false;
    }

    public static bool TryGetAsset<T>(string bundleName, string path, out T asset) where T : Object
    {
        string assetPath = $"{bundleName}:{path}";
        asset = default;

        if (!Assets.ContainsKey(assetPath))
        {
            BlenderAPI.LogWarning($"Couldn't find an asset with path {assetPath}.");
            return false;
        }

        if (Assets.TryGetValue(assetPath, out var asst) && asst is T t)
        {
            asset = t;
            return true;
        }

        BlenderAPI.LogError($"An asset with path {assetPath} couldn't be converted to type " +
            $"{typeof(T).Name}.");
        return false;
    }

    public static T CacheAsset<T>(string modName, string bundleName, string path) where T : Object
    {
        if (TryGetAsset(bundleName, path, out T t))
            return t;
        if (!ContainsBundle(bundleName))
            LoadBundle(modName, bundleName);

        string assetPath = $"{bundleName}:{path}";
        if (TryGetBundle(bundleName, out AssetBundle bundle))
        {
            T asset = bundle.LoadAsset<T>(path);
            Assets.Add(assetPath, asset);
            UnloadBundle(bundleName, false);
            return asset;
        }

        BlenderAPI.LogWarning($"Failed to cache an asset with path {assetPath}.");
        return default;
    }

    public static bool ContainsBundle(string bundleName)
    {
        return Bundles.ContainsKey(bundleName);
    }

    public static bool ContainsAsset(string bundleName, string path)
    {
        return Assets.ContainsKey($"{bundleName}:{path}");
    }

    public static List<AssetBundle> GetBundles()
    {
        return Bundles.Values.ToList();
    }

    public static List<Object> GetAssets()
    {
        return Assets.Values.ToList();
    }
}