using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System;
using System.Collections.Generic;
using static Blender.Utility.AssetHelper;

namespace Blender.Content;

public static class SceneRegistries
{
    public static readonly LinkedRegistry<Scenes, LevelInfo> Levels = new(
        (scene, info) =>
        {
            Levels level = (Levels)LevelsRegistry.Register(info.LevelName);
            LevelSceneLinker[level] = scene;
            AssetHelper.AddScenePathMapping(LoaderType.Single, scene.ToString(), [info.BundlePath]);
        });

    public static readonly LinkedRegistry<DialoguerDialogues, DialoguerDialogue> Dialogues = new(
        (instance, dialogue) =>
        {
            foreach (AbstractDialoguePhase phase in dialogue.phases)
            {
                if (phase is TextPhase textPhase)
                {
                    Traverse<int> traverse = Traverse.Create(textPhase.data).Field<int>("dialogueID");
                    traverse.Value = (int)instance;
                }
            }
        });

    internal static readonly EnumRegistry<Levels> LevelsRegistry = EnumManager.Register<Levels>();
    internal static readonly Dictionary<Levels, Scenes> LevelSceneLinker = [];

    public static Levels GetLevel(Scenes scene)
    {
        LevelInfo info = Levels.GetValue(scene.ToString());
        if (info != null)
            return (Levels)Enum.Parse(typeof(Levels), info.LevelName);
        return global::Levels.Slime;
    }
}