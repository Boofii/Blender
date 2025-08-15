using Blender.Patching;
using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Blender.Utility.AssetHelper;

namespace Blender.Content;

public static class SceneRegistries
{
    public static readonly LinkedRegistry<Scenes, LevelInfo> Levels = new(
        (scene, info) => {
            Levels level = (Levels)LevelsRegistry.Register(info.LevelName);
            LevelSceneLinker[level] = scene;
            AssetHelper.AddScenePathMapping(LoaderType.Single, scene.ToString(), [info.BundlePath]);
        });

    // Don't use this, it's not ready yet.
    /* public static readonly LinkedRegistry<Scenes, MapInfo> Maps = new(
        (scene, info) => {
            AssetHelper.AddScenePathMapping(LoaderType.Single, scene.ToString(), [info.BundlePath]);
        });
    */

    public static readonly LinkedRegistry<DialoguerDialogues, DialoguerDialogue> Dialogues = new(
        (instance, dialogue) => {
            foreach (AbstractDialoguePhase phase in dialogue.phases)
            {
                if (phase is TextPhase textPhase)
                {
                    Traverse<int> traverse = Traverse.Create(textPhase.data).Field<int>("dialogueID");
                    traverse.Value = (int)instance;
                }
            }
        });

    public static readonly ListeningDictionary<string, MapEntityInfo> MapEntities = new(
        (name, info) => {
            foreach (string scene in info.Scenes)
            {
                if (ProcessedEntityBundles.ContainsKey(scene) && ProcessedEntityBundles[scene].Contains(info.BundlePath))
                    continue;

                AssetHelper.AddScenePathMapping(LoaderType.Multiple, scene, [info.BundlePath]);

                if (ProcessedEntityBundles.TryGetValue(scene, out List<string> bundles))
                    bundles.Add(info.BundlePath);
                else
                    ProcessedEntityBundles.Add(scene, [info.BundlePath]);

            }

            if (MultiLoader.LoadActions[info.BundlePath] != null)
                return;

            MultiLoader.LoadActions[info.BundlePath] += (name, entities) => {
                foreach (UnityEngine.Object obj in entities)
                {
                    if (obj is not GameObject entity)
                        continue;

                    if (!MapEntities.ContainsKey(entity.name))
                    {
                        BlenderAPI.LogWarning($"Tried to load a map entity named \"{entity.name}\" that didn't have a matching entity.");
                        continue;
                    }

                    ScenePatcher.LoadedEntities.Add(entity.name, entity);
                }
            };
        });

    internal static readonly EnumRegistry<Levels> LevelsRegistry = EnumManager.Register<Levels>();
    internal static readonly Dictionary<Levels, Scenes> LevelSceneLinker = [];

    private static readonly Dictionary<string, List<string>> ProcessedEntityBundles = [];

    public static Levels GetLevel(Scenes scene)
    {
        LevelInfo info = Levels.GetValue(scene.ToString());
        if (info != null)
            return (Levels)Enum.Parse(typeof(Levels), info.LevelName);
        return global::Levels.Slime;
    }
}