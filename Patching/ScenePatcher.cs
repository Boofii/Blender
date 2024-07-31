using Blender.Content;
using Blender.Utility;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blender.Patching;

internal static class ScenePatcher {
    [HarmonyPatch(typeof(LevelProperties), nameof(LevelProperties.GetLevelScene))]
    [HarmonyPrefix]
    private static bool Patch_GetLevelScene(Levels level, ref string __result) {
        if (SceneRegistries.LevelSceneLinker.TryGetValue(level, out Scenes scene)) {
            __result = scene.ToString();
            return false;
        }
        return true;
    }

    private static void SetupCustomLevel(Scene scene, LoadSceneMode mode) {
        if (SceneRegistries.Levels.ContainsName(scene.name)) {
            LevelInfo info = SceneRegistries.Levels.GetValue(scene.name);
            GameObject levelObj = GameObject.Find("Level");
            levelObj.SetActive(false);
            Level level = (Level)levelObj.AddComponent(info.LevelType);
            LevelResources resources = AssetHelper.GetPrefab("Level_Resources").GetComponent<LevelResources>();
            level.LevelResources = resources;
            level.type = info.ActualType;
            level.playerMode = info.PlayerMode;
            level.goalTimes = info.DefaultGoalTimes;
            level.spawns = info.Spawns;
            level.intro = new();
            info.SetupAction?.Invoke(level);
            levelObj.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(MapLevelLoader), nameof(MapLevelLoader.Awake))]
    [HarmonyPostfix]
    private static void Patch_MapLevelLoaderAwake(MapLevelLoader __instance) {
        if (__instance.level == Levels.Slime)
            __instance.level = (Levels)Enum.Parse(typeof(Levels), "Custom");
    }

    internal static void Initialize(Harmony harmony) {
        harmony.PatchAll(typeof(ScenePatcher));
        SceneManager.sceneLoaded += SetupCustomLevel;
    }
}