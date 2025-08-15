using Blender.Content;
using Blender.Utility;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blender.Patching;

internal static class ScenePatcher
{
    public static readonly Dictionary<string, GameObject> LoadedEntities = [];

    [HarmonyPatch(typeof(LevelProperties), nameof(LevelProperties.GetLevelScene))]
    [HarmonyPrefix]
    private static bool Patch_GetLevelScene(Levels level, ref string __result)
    {
        if (SceneRegistries.LevelSceneLinker.TryGetValue(level, out Scenes scene))
        {
            __result = scene.ToString();
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Map), nameof(Map.Awake))]
    [HarmonyPostfix]
    private static void Patch_OnMapAwake()
    {
        foreach (string name in LoadedEntities.Keys)
        {
            GameObject prefab = LoadedEntities[name];
            MapEntityInfo info = SceneRegistries.MapEntities[name];
            GameObject clone = GameObject.Instantiate(prefab, info.Position, info.Rotation);
            info.SetupAction?.Invoke(clone);

            SpriteRenderer renderer = clone.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.sortingLayerName = "Map";
            }

            if (info.DialoguerDialogues != null)
            {
                clone.SetActive(false);
                MapDialogueInteraction interaction = clone.AddComponent<MapDialogueInteraction>();
                interaction.speechBubblePrefab = ScenePatcher.GetSpeechBubble();
                interaction.dialogueInteraction = info.DialoguerDialogues.Value;
                interaction.dialogueProperties = info.DialogueProperties;
                interaction.dialogueOffset = info.DialogueOffset;
                interaction.interactionDistance = info.InteractionDistance;
                interaction.speechBubblePosition = info.SpeechBubbleOffset;
                clone.SetActive(true);
            }
        }
        LoadedEntities.Clear();
    }

    private static void SetupCustomScene(Scene scene, LoadSceneMode mode)
    {
        if (SceneRegistries.Levels.ContainsName(scene.name))
        {
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
        /*else if (SceneRegistries.Maps.ContainsName(scene.name))
        {
            MapInfo info = SceneRegistries.Maps.GetValue(scene.name);
            GameObject mapObj = GameObject.Find("Map");
            mapObj.SetActive(false);
            Map map = mapObj.AddComponent<Map>();
            MapResources resources = AssetHelper.GetPrefab("Map_Resources").GetComponent<MapResources>();
            map.MapResources = resources;
            map.firstNode = GameObject.Find(info.FirstNode).GetComponent<AbstractMapInteractiveEntity>();
            map.cameraProperties = info.CameraProperties;
            info.SetupAction?.Invoke(map);
            mapObj.SetActive(true);
        }*/
    }

    private static SpeechBubble GetSpeechBubble()
    {
        GameObject root = GameObject.Find("Entities");
        if (root != null)
        {
            MapDialogueInteraction firstEntity = root.GetComponentInChildren<MapDialogueInteraction>();
            return firstEntity.speechBubblePrefab;
        }
        return null;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(ScenePatcher));
        SceneManager.sceneLoaded += SetupCustomScene;
    }
}