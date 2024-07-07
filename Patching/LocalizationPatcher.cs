using Blender.Content;
using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(Localization))]
public static class LocalizationPatcher
{
    internal static readonly List<Identifier> RegisteredIds = [];
    private static readonly Dictionary<string, TranslationElement> Elements = [];
    private static readonly Dictionary<string, TranslationElement> DuplicatedElements = [];

    private static bool TryGetField<T>(Identifier id, SimpleJson.JsonObject json, string name, out T value, bool required)
    {
        value = default;
        if (!json.ContainsKey(name))
        {
            if (required)
                BlenderAPI.LogError($"A localization file with path \"{id}\" had a missing \"{name}\" field.");
            return false;
        }
        object val = json[name];
        if (val is SimpleJson.JsonObject obj) {
            T t = SimpleJson.SimpleJson.DeserializeObject<T>(obj.ToString());
            value = t;
            return true;
        }
        else
        {
            value = (T)val;
            return true;
        }
    }

    [HarmonyPatch(nameof(Localization.Awake))]
    [HarmonyPostfix]
    private static void Patch_Awake()
    {
        foreach (Identifier id in RegisteredIds)
        {
            if (id.Validate())
            {
                string json = File.ReadAllText(id.ActualPath);
                var result = SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonArray>(json);
                if (result != null)
                {
                    foreach (var keyObj in result)
                    {
                        if (keyObj is SimpleJson.JsonObject keyJson) {
                            string key = null;
                            Localization.Categories category = Localization.Categories.NoCategory;
                            SimpleJson.JsonObject translations = null;
                            TranslationElement element = new();

                            if (TryGetField(id, keyJson, "key", out string _key, true))
                                key = _key;
                            if (TryGetField(id, keyJson, "category", out string _category, false))
                                category = (Localization.Categories)Enum.Parse(typeof(Localization.Categories), _category);
                            if (TryGetField(id, keyJson, "translations", out SimpleJson.JsonObject _translations, true))
                            {
                                translations = _translations;
                            }

                            if (key != null && translations != null)
                            {
                                int langCount = Enum.GetValues(typeof(Localization.Languages)).Length;
                                var results = new Localization.Translation[langCount];
                                foreach (string langStr in translations.Keys)
                                {
                                    var lang = (Localization.Languages)Enum.Parse(typeof(Localization.Languages), langStr);
                                    Localization.Translation translation = new();

                                    if (TryGetField(id, translations, langStr, out SimpleJson.JsonObject translationJson, false))
                                    {
                                        string text = null;

                                        if (TryGetField(id, translationJson, "text", out string _text, true))
                                            text = _text;

                                        translation.text = text;
                                        results[(int)lang] = translation;
                                    }
                                }

                                for (int i = 0; i < results.Length; i++)
                                {
                                    if (results[i].Equals(default(Localization.Translation)))
                                    {
                                        results[i] = new()
                                        {
                                            text = "ERROR"
                                        };
                                    }
                                }

                                element.key = key;
                                element.category = category;
                                element.depth = 0;
                                element.id = LargestId + 1;
                                element.translations = results;

                                if (!Elements.ContainsKey(key) && Localization.Instance.m_TranslationElements.Find(elm => elm.key == key) == null)
                                    Elements.Add(key, element);
                                else
                                    DuplicatedElements.Add(key, element);
                            }
                        }
                    }
                }
            }
            else
            {
                BlenderAPI.LogWarning($"A localization file with path \"{id}\" couldn't be found.");
            }
        }
        foreach (TranslationElement translation in Elements.Values)
            Localization.Instance.m_TranslationElements.Add(translation);
    }

    [HarmonyPatch(typeof(DialoguerDataManager), nameof(DialoguerDataManager.Initialize))]
    [HarmonyPostfix]
    private static void Patch_DialoguerInit()
    {
        foreach (DialoguerDialogue dialogue in SceneRegistries.Dialogues.GetValues())
            DialoguerDataManager._data.dialogues.Add(dialogue);
    }

    private static int LargestId {
        get
        {
            return Localization.Instance.m_TranslationElements.Count;
        }
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(LocalizationPatcher));
    }
}