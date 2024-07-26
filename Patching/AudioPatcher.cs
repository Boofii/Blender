using Blender.Utility;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static AudioManagerComponent;

namespace Blender.Patching;

public static class AudioPatcher
{
    private static readonly Dictionary<string, bool> audio = [];

    [HarmonyPatch(typeof(AudioManagerComponent), nameof(AudioManagerComponent.Awake))]
    [HarmonyPrefix]
    private static void Patch_AudioAwake(AudioManagerComponent __instance)
    {
        if (__instance.GetComponent<LevelAudio>() != null || __instance.transform.parent.GetComponent<Map>() != null)
        {
            Transform soundHolder = new GameObject("Custom_SFX").transform;
            soundHolder.parent = __instance.transform;
            Transform bgmHolder = new GameObject("Custom_BGM").transform;
            bgmHolder.parent = __instance.transform;

            foreach (string bundlePath in AssetLoader<Object[]>.Instance.loadedAssets.Keys)
            {
                Object[] assets = AssetLoader<Object[]>.Instance.loadedAssets[bundlePath].asset;
                foreach (Object asset in assets)
                {
                    if (asset is not AudioClip clip || !audio.ContainsKey(bundlePath))
                        continue;

                    GameObject audioObj = new(clip.name);
                    if (audio[bundlePath] == false)
                    {
                        audioObj.transform.parent = soundHolder;
                        AudioSource source = audioObj.AddComponent<AudioSource>();
                        source.clip = clip;
                        SoundGroup group = new()
                        {
                            key = clip.name,
                            sources = [new() { audio = source }]
                        };
                        __instance.sounds.Add(group);
                    }
                    else
                    {
                        audioObj.transform.parent = bgmHolder;
                        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
                        audioSource.clip = clip;
                        SoundGroup.Source source = new()
                        {
                            audio = audioSource
                        };
                        __instance.bgmSources.Add(source);
                    }
                }
            }
        }
    }

    public static void AddSounds(string scene, string bundlePath)
    {
        AssetHelper.AddScenePathMapping(AssetHelper.LoaderType.Multiple, scene, [bundlePath]);
        if (!audio.ContainsKey(bundlePath))
            audio.Add(bundlePath, false);
    }

    public static void AddBGM(string scene, string bundlePath)
    {
        AssetHelper.AddScenePathMapping(AssetHelper.LoaderType.Multiple, scene, [bundlePath]);
        if (!audio.ContainsKey(bundlePath))
            audio.Add(bundlePath, true);
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(AudioPatcher));
    }
}