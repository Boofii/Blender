using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blender.Utility;

public class SingleLoader : AssetLoader<UnityEngine.Object>
{
    public static readonly Dictionary<string, Action<string, UnityEngine.Object>> LoadActions = [];
    public static readonly Dictionary<string, Action<string, UnityEngine.Object>> DestroyActions = [];
    public static readonly Dictionary<UnityEngine.Object, string> AssetPaths = [];

    public override Coroutine loadAsset(string assetName, Action<UnityEngine.Object> completionHandler)
    {
        AssetBundleLoader.AssetBundleLocation location = AssetHelper.ModAssetsLocation;
        string[] splitName = assetName.Split(AssetHelper.AssetSeperator);
        string bundleName = splitName[0];
        if (splitName.Length < 2)
        {
            return AssetBundleLoader.Instance.StartCoroutine
                (AssetBundleLoader.Instance.loadAssetBundle(bundleName, location));
        }
        string name = splitName[1];
        return AssetBundleLoader.Instance.StartCoroutine(AssetBundleLoader.Instance.loadAsset(bundleName, location, name,
            new Action<UnityEngine.Object>((asset) =>
            {
                LoadActions[assetName]?.Invoke(assetName, asset);
                completionHandler?.Invoke(asset);
                if (!AssetPaths.ContainsKey(asset))
                    AssetPaths.Add(asset, assetName);
            })));
    }

    public override UnityEngine.Object loadAssetSynchronous(string assetName)
    {
        throw new NotImplementedException();
    }

    public override void destroyAsset(UnityEngine.Object asset)
    {
        string path = AssetPaths[asset];
        DestroyActions[path]?.Invoke(path, asset);
        if (AssetPaths.ContainsKey(asset))
            AssetPaths.Remove(asset);
        Destroy(asset);
    }
}