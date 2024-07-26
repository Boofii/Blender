using BepInEx;
using Blender.Patching;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static AssetBundleLoader;
using static RuntimeSceneAssetDatabase;

namespace Blender.Utility;

public static class AssetHelper
{
    public static AssetBundleLocation ModAssetsLocation;
    public static readonly char BundleSeperator = ':';
    public static readonly char AssetSeperator = '\\';
    private static readonly string LocationName = "ModAssets";
    private static readonly List<string> persistentAssets = [];
    private static readonly Dictionary<string, string[]> sceneAssetMappings = [];
    private static readonly List<string> persistentBundles = [];
    private static readonly Dictionary<string, string[]> sceneBundleMappings = [];
    private static GameObject PrefabHolder { get; set; }
    private static readonly List<string> PrefabKeys = [];

    [HarmonyPatch(typeof(AssetBundleLoader), nameof(AssetBundleLoader.Awake))]
    [HarmonyPostfix]
    private static void Patch_LoaderAwake(AssetBundleLoader __instance)
    {
        CustomLoader customLoader = __instance.gameObject.AddComponent<CustomLoader>();
        RuntimeSceneAssetDatabase database = ScriptableObject.CreateInstance<RuntimeSceneAssetDatabase>();
        database.name = "SceneAssetDatabase";
        SceneAssetMapping[] mappings = sceneAssetMappings.Select((mapping) =>
        {
            return new SceneAssetMapping()
            {
                sceneName = mapping.Key,
                assetNames = mapping.Value
            };
        }).ToArray();
        database.INTERNAL_persistentAssetNames = persistentAssets.ToArray();
        database.INTERNAL_sceneAssetMappings = mappings;
        database.INTERNAL_persistentAssetNamesDLC = [];
        customLoader.sceneAssetDatabase = database;

        AssetLocationDatabase locationDatabase = ScriptableObject.CreateInstance<AssetLocationDatabase>();
        locationDatabase.name = "AssetLocationDatabase";
        locationDatabase.dlcAssetNames = [];
        customLoader.assetLocationDatabase = locationDatabase;

        MultiLoader multiLoader = __instance.gameObject.AddComponent<MultiLoader>();
        RuntimeSceneAssetDatabase database1 = ScriptableObject.CreateInstance<RuntimeSceneAssetDatabase>();
        database1.name = "SceneAssetDatabase";
        SceneAssetMapping[] mappings1 = sceneBundleMappings.Select((mapping) =>
        {
            return new SceneAssetMapping()
            {
                sceneName = mapping.Key,
                assetNames = mapping.Value
            };
        }).ToArray();
        database1.INTERNAL_persistentAssetNames = persistentBundles.ToArray();
        database1.INTERNAL_sceneAssetMappings = mappings1;
        database1.INTERNAL_persistentAssetNamesDLC = [];
        multiLoader.sceneAssetDatabase = database1;

        AssetLocationDatabase locationDatabase1 = ScriptableObject.CreateInstance<AssetLocationDatabase>();
        locationDatabase1.name = "AssetLocationDatabase";
        locationDatabase1.dlcAssetNames = [];
        multiLoader.assetLocationDatabase = locationDatabase1;
    }

    [HarmonyPatch(typeof(AssetBundleLoader), nameof(AssetBundleLoader.loadAssetBundle), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Patch_LinkModName(IEnumerable<CodeInstruction> instructions)
    {
        bool foundFirst = false;
        Type type = AccessTools.Inner(typeof(AssetBundleLoader), "<loadAssetBundle>c__Iterator0");
        foreach (var instruction in instructions)
        {
            if (instruction.operand is string str && str == "AssetBundles")
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.LoadField(type, "assetBundleName");
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.LoadField(type, "location");
                yield return CodeInstruction.Call(typeof(AssetHelper), nameof(GetBundlePath));
            }
            else if (instruction.LoadsField(AccessTools.Field(type, "assetBundleName")) && !foundFirst)
            {
                foundFirst = true;
                yield return CodeInstruction.LoadField(type, "assetBundleName");
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.LoadField(type, "location");
                yield return CodeInstruction.Call(typeof(AssetHelper), nameof(GetBundleName));
            }
            else
                yield return instruction;
        }
    }

    [HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.load_cr), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Patch_SceneLoad(IEnumerable<CodeInstruction> instructions)
    {
        bool foundFirst = false;
        Type type = AccessTools.Inner(typeof(SceneLoader), "<load_cr>d__0");
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode == OpCodes.Blt && !foundFirst)
            {
                foundFirst = true;
                yield return CodeInstruction.Call(typeof(AssetHelper), nameof(LoadCustomAssets));
            }
            else if (instruction.Calls(AccessTools.Method(typeof(AssetLoader<Texture2D[]>), nameof(AssetLoader<Texture2D[]>.UnloadAssets))))
            {
                yield return CodeInstruction.Call(typeof(AssetHelper), nameof(UnloadCustomAssets));
            }
        }
    }

    private static void LoadCustomAssets()
    {
        AssetLoaderOption option = AssetLoaderOption.None();
        string[] preloadCustoms = AssetLoader<UnityEngine.Object>.GetPreloadAssetNames(SceneLoader.SceneName);
        for (int i = 0; i < preloadCustoms.Length; i++)
            AssetLoader<UnityEngine.Object>.LoadAsset(preloadCustoms[i], option);
        string[] preloadMultiples = AssetLoader<UnityEngine.Object[]>.GetPreloadAssetNames(SceneLoader.SceneName);
        for (int i = 0; i < preloadMultiples.Length; i++)
            AssetLoader<UnityEngine.Object[]>.LoadAsset(preloadMultiples[i], option);
    }

    private static void UnloadCustomAssets()
    {
        AssetLoader<UnityEngine.Object>.UnloadAssets([]);
        AssetLoader<UnityEngine.Object[]>.UnloadAssets([]);
    }

    private static string GetBundlePath(string bundleName, AssetBundleLocation location)
    {
        if (location == ModAssetsLocation)
        {
            string[] splitName = bundleName.Split(BundleSeperator);
            return Path.Combine(splitName[0], "Assets");
        }
        return "AssetBundles";
    }

    private static string GetBundleName(string bundleName, AssetBundleLocation location)
    {
        if (location == ModAssetsLocation)
        {
            string[] splitName = bundleName.Split(BundleSeperator);
            return splitName[1];
        }
        return bundleName;
    }

    [HarmonyPatch(typeof(AssetBundleLoader), nameof(getBasePath))]
    [HarmonyPrefix]
    private static bool Patch_GetBasePath(AssetBundleLocation location, ref string __result)
    {
        if (location == ModAssetsLocation)
        {
            __result = Paths.PluginPath;
            return false;
        }
        return true;
    }

    public static void AddPersistentPath(LoaderType type, string path)
    {
        if (type == LoaderType.Single)
        {
            if (!persistentAssets.Contains(path))
            {
                persistentAssets.Add(path);
                CustomLoader.LoadActions[path] = null;
                CustomLoader.DestroyActions[path] = null;
            }
        }
        else
        {
            if (!persistentBundles.Contains(path))
            {
                persistentBundles.Add(path);
                MultiLoader.LoadActions[path] = null;
                MultiLoader.DestroyActions[path] = null;
            }
        }
    }

    public static void AddScenePathMapping(LoaderType type, string scene, string[] paths)
    {
        if (type == LoaderType.Single)
        {
            if (!sceneAssetMappings.ContainsKey(scene))
            {
                sceneAssetMappings.Add(scene, paths);
                foreach (string path in paths)
                {
                    CustomLoader.LoadActions[path] = null;
                    CustomLoader.DestroyActions[path] = null;
                }
            }
            else
            {
                sceneAssetMappings[scene] = sceneAssetMappings[scene].AddRangeToArray(paths);
                foreach (string path in paths)
                {
                    CustomLoader.LoadActions[path] = null;
                    CustomLoader.DestroyActions[path] = null;
                }
            }
        }
        else
        {
            if (!sceneBundleMappings.ContainsKey(scene))
            {
                sceneBundleMappings.Add(scene, paths);
                foreach (string path in paths)
                {
                    MultiLoader.LoadActions[path] = null;
                    MultiLoader.DestroyActions[path] = null;
                }
            }
            else
            {
                string[] newBundles = paths.Where(p => !sceneBundleMappings[scene].Contains(p)).ToArray();
                sceneBundleMappings[scene] = sceneBundleMappings[scene].AddRangeToArray(newBundles);
                foreach (string path in newBundles)
                {
                    MultiLoader.LoadActions[path] = null;
                    MultiLoader.DestroyActions[path] = null;
                }
            }
        }
    }

    public static GameObject AddPrefab(GameObject prefab, bool asClone)
    {
        if (PrefabKeys.Contains(prefab.name))
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
        PrefabKeys.Add(newPrefab.name);
        return newPrefab;
    }

    public static GameObject GetPrefab(string name)
    {
        Transform prefab = PrefabHolder.transform.Find(name);
        if (prefab == null)
        {
            BlenderAPI.LogWarning($"Couldn't find a prefab with name \"{name}\".");
            return null;
        }
        return prefab.gameObject;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(AssetHelper));
        PrefabHolder = new GameObject("PrefabHolder");
        GameObject.DontDestroyOnLoad(PrefabHolder);
        PrefabHolder.SetActive(false);
        EnumRegistry<AssetBundleLocation> locationReg = EnumManager.Register<AssetBundleLocation>();
        locationReg.Register(LocationName);
        ModAssetsLocation = (AssetBundleLocation)Enum.Parse(typeof(AssetBundleLocation), LocationName);
    }

    public enum LoaderType
    {
        Single,
        Multiple
    }
}