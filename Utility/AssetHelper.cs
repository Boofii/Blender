using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blender.Utility;

public static class AssetHelper
{
    private static readonly Dictionary<string, AssetBundle> Bundles = [];
    private static readonly Dictionary<string, UnityEngine.Object> Assets = [];
    private static GameObject PrefabHolder { get; set; }

    public static void LoadBundle(Identifier id, Action<AssetBundle> completionEvent)
    {
        if (id == null)
            return;

        string str = id.ToString();
        if (!Bundles.ContainsKey(str) && id.Validate())
        {
            var request = AssetBundle.LoadFromFileAsync(id.ActualPath);
            AssetBundle bundle = request.assetBundle;
            if (bundle == null)
            {
                BlenderAPI.LogWarning($"Tried to load a bundle that didn't exist with path \"{str}\".");
                return;
            }
            Bundles.Add(str, bundle);
            completionEvent?.Invoke(bundle);
            return;
        }
        completionEvent?.Invoke(Bundles[str]);
    }

    public static void UnloadBundle(Identifier id, bool unloadAllLoadedObjects)
    {
        string str = id.ToString();
        if (Bundles.TryGetValue(str, out var bundle))
        {
            bundle.Unload(unloadAllLoadedObjects);
            Bundles.Remove(str);
            return;
        }
        BlenderAPI.LogWarning($"A bundle with path \"{str}\" didn't exist in the dictionary.");
    }

    public static void LoadAsset<T>(Identifier id, string name, Action<T> completionEvent) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(name) || id == null)
            return;

        string str = id.ToString();
        string assetStr = id.Combine(name).ToString();
        if (!Assets.ContainsKey(assetStr))
        {
            LoadBundle(id, (bundle) =>
            {
                var request = bundle.LoadAssetAsync<T>(name);
                T asset = (T)request.asset;
                if (asset == null)
                {
                    BlenderAPI.LogWarning($"An asset with path \"{assetStr}\" was null or was not of type \"{typeof(T).Name}\".");
                    return;
                }
                if (asset is GameObject obj)
                {
                    SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                        renderer.material = new Material(Shader.Find("Sprites/Default"));
                }

                completionEvent?.Invoke(asset);
                UnloadBundle(id, false);
            });
            return;
        }
        completionEvent?.Invoke((T)Assets[assetStr]);
    }

    public static void CacheAsset<T>(Identifier id, string name, Action<T> completionEvent) where T : UnityEngine.Object
    {
        LoadAsset<T>(id, name, (asset) =>
        {
            string assetStr = id.Combine(name).ToString();
            Assets[assetStr] = asset;
            completionEvent?.Invoke(asset);
        });
    }

    public static GameObject AddPrefab(GameObject prefab, bool asClone)
    {
        if (GetPrefab(prefab.name) != null)
        {
            BlenderAPI.LogWarning($"Tried to add a prefab that already existed with name \"{prefab.name}\".");
            return null;
        }

        GameObject newPrefab = prefab;
        if (asClone)
        {
            newPrefab = GameObject.Instantiate(prefab);
            newPrefab.name = prefab.name;
        }
        newPrefab.transform.SetParent(PrefabHolder.transform);
        return newPrefab;
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

    internal static void Initialize()
    {
        PrefabHolder = new GameObject("PrefabHolder");
        GameObject.DontDestroyOnLoad(PrefabHolder);
        PrefabHolder.SetActive(false);
    }
}