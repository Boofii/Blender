using Blender.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blender.Patching;

public class CustomLoader : AssetLoader<UnityEngine.Object>
{
    public static readonly Dictionary<string, Action<string, UnityEngine.Object>> LoadActions = [];
    public static readonly Dictionary<string, Action<string, UnityEngine.Object>> DestroyActions = [];
    private static readonly Dictionary<UnityEngine.Object, string> AssetPaths = [];

    public override Coroutine loadAsset(string assetName, Action<UnityEngine.Object> completionHandler)
    {
        AssetBundleLoader.AssetBundleLocation location = AssetHelper.ModAssetsLocation;
        string[] splitName = assetName.Split(AssetHelper.AssetSeperator);
        if (splitName.Length != 2)
        {
            BlenderAPI.LogWarning($"Tried to load an asset named \"{assetName}\" that consisted of more/less elements than 2.");
            return null;
        }
        string bundleName = splitName[0];
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