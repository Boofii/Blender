using BepInEx;
using Blender.Content;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using static AssetBundleLoader;
using static RuntimeSceneAssetDatabase;
using static SceneLoader;

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

    public static void AddPersistentPath(LoaderType type, string path)
    {
        if (type == LoaderType.Single)
        {
            if (!persistentAssets.Contains(path))
            {
                persistentAssets.Add(path);
                SingleLoader.LoadActions[path] = null;
                SingleLoader.DestroyActions[path] = null;
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
                    SingleLoader.LoadActions[path] = null;
                    SingleLoader.DestroyActions[path] = null;
                }
            }
            else
            {
                sceneAssetMappings[scene] = sceneAssetMappings[scene].AddRangeToArray(paths);
                foreach (string path in paths)
                {
                    SingleLoader.LoadActions[path] = null;
                    SingleLoader.DestroyActions[path] = null;
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

    [HarmonyPatch(typeof(AssetBundleLoader), nameof(AssetBundleLoader.Awake))]
    [HarmonyPostfix]
    private static void Patch_LoaderAwake(AssetBundleLoader __instance)
    {
        SingleLoader customLoader = __instance.gameObject.AddComponent<SingleLoader>();
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

    [HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.load_cr))]
    [HarmonyPrefix]
    private static bool Patch_SceneLoad(SceneLoader __instance, ref IEnumerator __result)
    {
        __result = HandleSceneLoad(__instance);
        return false;
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

    public static bool HasPrefab(string name) {
        Transform prefab = PrefabHolder.transform.Find(name);
        return prefab != null;
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

    public static void RemovePrefab(string name) {
        Transform prefab = PrefabHolder.transform.Find(name);
        if (prefab != null) {
            PrefabKeys.Remove(prefab.name);
            GameObject.Destroy(prefab.gameObject);
        }
    }

    private static IEnumerator HandleSceneLoad(SceneLoader instance)
    {
        instance.doneLoadingSceneAsync = false;
        GC.Collect();
        if (SceneName != previousSceneName && SceneName != Scenes.scene_slot_select.ToString())
        {
            string text = null;
            if (!Array.Exists(Level.kingOfGamesLevelsWithCastle, (Levels level) => LevelProperties.GetLevelScene(level) == SceneName))
            {
                text = Scenes.scene_level_chess_castle.ToString();
            }

            AssetBundleLoader.UnloadAssetBundles();
            AssetLoader<SpriteAtlas>.UnloadAssets(text);
            if (SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
            {
                AssetLoader<AudioClip>.UnloadAssets();
            }

            AssetLoader<Texture2D[]>.UnloadAssets();
            AssetLoader<UnityEngine.Object>.UnloadAssets();
            AssetLoader<UnityEngine.Object[]>.UnloadAssets();
            AssetHelper.RemovePrefab("Level_Resources");
        }

        if (SceneName == Scenes.scene_title.ToString())
        {
            DLCManager.RefreshDLC();
        }

        AssetLoaderOption atlasOption = AssetLoaderOption.None();
        if (SceneName == Scenes.scene_level_chess_castle.ToString())
        {
            atlasOption = AssetLoaderOption.PersistInCacheTagged(SceneName);
        }

        string[] preloadAtlases = AssetLoader<SpriteAtlas>.GetPreloadAssetNames(SceneName);
        string[] preloadMusic = AssetLoader<AudioClip>.GetPreloadAssetNames(SceneName);
        LevelInfo info = SceneRegistries.Levels.GetValue(SceneName);
        if (info != null)
            yield return instance.StartCoroutine(GetResources(info));

        if (SceneName != previousSceneName && (preloadAtlases.Length != 0 || preloadMusic.Length != 0 ||
            AssetLoader<UnityEngine.Object>.GetPreloadAssetNames(SceneName).Length != 0 ||
            AssetLoader<UnityEngine.Object[]>.GetPreloadAssetNames(SceneName).Length != 0))
        {
            AsyncOperation intermediateSceneAsyncOp = SceneManager.LoadSceneAsync(instance.LOAD_SCENE_NAME);
            while (!intermediateSceneAsyncOp.isDone)
            {
                yield return null;
            }

            for (int k = 0; k < preloadAtlases.Length; k++)
            {
                yield return AssetLoader<SpriteAtlas>.LoadAsset(preloadAtlases[k], atlasOption);
            }

            AssetLoaderOption musicOption = AssetLoaderOption.None();
            for (int k = 0; k < preloadMusic.Length; k++)
            {
                yield return AssetLoader<AudioClip>.LoadAsset(preloadMusic[k], musicOption);
            }

            AssetLoaderOption option = AssetLoaderOption.None();

            string[] preloadSingles = AssetLoader<UnityEngine.Object>.GetPreloadAssetNames(SceneLoader.SceneName);
            for (int i = 0; i < preloadSingles.Length; i++)
            {
                yield return AssetLoader<UnityEngine.Object>.LoadAsset(preloadSingles[i], option);
            }
            string[] preloadMultiples = AssetLoader<UnityEngine.Object[]>.GetPreloadAssetNames(SceneLoader.SceneName);
            for (int i = 0; i < preloadMultiples.Length; i++)
            {
                yield return AssetLoader<UnityEngine.Object[]>.LoadAsset(preloadSingles[i], option);
            }

            Coroutine[] persistentAssetsCoroutines = DLCManager.LoadPersistentAssets();
            if (persistentAssetsCoroutines != null)
            {
                for (int k = 0; k < persistentAssetsCoroutines.Length; k++)
                {
                    yield return persistentAssetsCoroutines[k];
                }
            }

            yield return null;
        }

        AsyncOperation async = SceneManager.LoadSceneAsync(SceneName);
        while (!async.isDone || AssetBundleLoader.loadCounter > 0)
        {
            instance.UpdateProgress(async.progress);
            yield return null;
        }

        instance.doneLoadingSceneAsync = true;
    }

    private static IEnumerator GetResources(LevelInfo info)
    {
        if (AssetHelper.HasPrefab("Level_Resources"))
            yield break;

        AsyncOperation request = SceneManager.LoadSceneAsync(info.ResourcesScene);
        while (!request.isDone)
            yield return null;

        Level level = GameObject.FindObjectOfType<Level>();
        LevelResources resources = level.LevelResources;
        resources.name = "Level_Resources";
        AssetHelper.AddPrefab(resources.gameObject, true);
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