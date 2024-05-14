using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Blender.Utility;

public static class AssetHelper
{

    private static readonly Dictionary<string, AssetBundle> Bundles = [];
    private static readonly Dictionary<string, Object> Assets = [];

    public static AssetBundle LoadBundle(string bundleName)
    {
        if (!Bundles.ContainsKey(bundleName))
        {
            string modDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string assetsPath = Path.Combine(modDir, "Assets");
            string bundlePath = Path.Combine(assetsPath, bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Logger.Error($"Couldn't find an asset bundle with path {bundlePath}");
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

        Logger.Error($"Couldn't find an asset bundle with name {bundleName}");
    }

    public static bool TryGetBundle(string bundleName, out AssetBundle bundle)
    {
        bundle = null;
        if (Bundles.TryGetValue(bundleName, out var bnd))
        {
            bundle = bnd;
            return true;
        }

        Logger.Error($"Couldn't find an asset bundle with name {bundleName}");
        return false;
    }

    public static bool TryGetAsset<T>(string bundleName, string path, out T asset) where T : Object
    {
        string assetPath = $"{bundleName}:{path}";
        asset = default;

        if (Assets.TryGetValue(assetPath, out var asst) && asst is T t)
        {
            asset = t;
            return true;
        }

        Logger.Error($"Couldn't find an asset with path {path}");
        return false;
    }

    public static T CacheAsset<T>(string bundleName, string assetName) where T : Object
    {
        if (TryGetAsset(bundleName, assetName, out T ast))
            return ast;
        if (!ContainsBundle(bundleName))
            LoadBundle(bundleName);

        T asset = default;
        if (TryGetBundle(bundleName, out AssetBundle bundle))
        {
            AssetBundleRequest assetRequest = bundle.LoadAssetAsync<T>(assetName);
            assetRequest.completed += delegate
            {
                asset = (T)assetRequest.asset;
                Assets.Add($"{bundleName}:{assetName}", asset);
                UnloadBundle(bundleName, false);
            };
        }
        return asset;
    }

    public static bool ContainsBundle(string bundleName)
    {
        return Bundles.ContainsKey(bundleName);
    }

    public static bool ContainsAsset(string path)
    {
        return Assets.ContainsKey(path);
    }

    public static List<AssetBundle> GetBundles()
    {
        return Bundles.Values.ToList();
    }

    public static List<Object> GetAssets()
    {
        return Assets.Values.ToList();
    }

    private static void AddAsset(string path, Object asset)
    {
        if (Assets.ContainsKey(path))
            return;
        Assets.Add(path, asset);
    }
}