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
    private static GameObject PrefabHolder { get; set; }

    public static string LoadFile(string modName, string path)
    {
        string modDir = Path.Combine(Paths.PluginPath, modName);
        string assetsPath = Path.Combine(modDir, "Assets");
        string filePath = Path.Combine(assetsPath, path);
        if (!File.Exists(filePath))
        {
            BlenderAPI.LogWarning($"Couldn't find a file with path {filePath}.");
            return null;
        }
        return filePath;
    }

    public static AssetBundle LoadBundle(string modName, string bundleName)
    {
        if (!Bundles.ContainsKey(bundleName))
        {
            string bundlePath = LoadFile(modName, bundleName);
            if (bundlePath != null)
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    BlenderAPI.LogWarning($"Couldn't find an asset bundle with path {bundlePath}.");
                    return null;
                }
                Bundles.Add(bundleName, bundle);
                return bundle;
            }
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
        if (bundleName == string.Empty || path == string.Empty)
            return null;

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

    public static T CacheAndSave<T>(string modName, string bundleName, string path) where T : Object
    {
        string assetPath = $"{bundleName}:{path}";

        T asset = CacheAsset<T>(modName, bundleName, path);
        if (asset is not GameObject obj)
            BlenderAPI.LogError($"Tried to save an asset with path {assetPath} "
                + "that wasn't a GameObject.");
        else
            AddPrefab(obj);

        return asset;
    }

    public static GameObject AddPrefab(GameObject prefab)
    {
        if (GetPrefab(prefab.name) != null)
            BlenderAPI.LogWarning($"Tried to add a prefab that already existed " +
                $"with name {prefab.name}.");

        prefab.transform.SetParent(PrefabHolder.transform);
        return prefab;
    }

    public static GameObject GetPrefab(string name)
    {
        Transform prefab = PrefabHolder.transform.Find(name);
        if (prefab == null)
        {
            BlenderAPI.LogWarning($"Couldn't find a prefab with name {name}.");
            return null;
        }
        return prefab.gameObject;
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

    internal static void Initialize()
    {
        PrefabHolder = new GameObject("PrefabHolder");
        GameObject.DontDestroyOnLoad(PrefabHolder);
        PrefabHolder.SetActive(false);
    }
}