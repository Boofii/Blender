using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blender.Utility;

public class MultiLoader : AssetLoader<UnityEngine.Object[]>
{
    public static readonly Dictionary<string, Action<string, UnityEngine.Object[]>> LoadActions = [];
    public static readonly Dictionary<string, Action<string, UnityEngine.Object[]>> DestroyActions = [];
    private static readonly Dictionary<UnityEngine.Object[], string> AssetPaths = [];

    public override Coroutine loadAsset(string bundleName, Action<UnityEngine.Object[]> completionHandler)
    {
        AssetBundleLoader.AssetBundleLocation location = AssetHelper.ModAssetsLocation;
        return AssetBundleLoader.Instance.StartCoroutine(AssetBundleLoader.Instance.loadAllAssets(bundleName, location,
            new Action<UnityEngine.Object[]>((assets) =>
            {
                LoadActions[bundleName]?.Invoke(bundleName, assets);
                completionHandler?.Invoke(assets);
                if (!AssetPaths.ContainsKey(assets))
                    AssetPaths.Add(assets, bundleName);
            })));
    }

    public override UnityEngine.Object[] loadAssetSynchronous(string assetName)
    {
        throw new NotImplementedException();
    }

    public override void destroyAsset(UnityEngine.Object[] assets)
    {
        string path = AssetPaths[assets];
        DestroyActions[path]?.Invoke(path, assets);
        if (AssetPaths.ContainsKey(assets))
            AssetPaths.Remove(assets);
        foreach (UnityEngine.Object asset in assets)
            Destroy(asset);
    }
}