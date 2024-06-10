using BepInEx;
using Blender.Utility;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace Blender.Patching;

[HarmonyPatch(typeof(Localization))]
internal static class LocalizationPatcher
{
    private static readonly List<TranslationElement> Elements = [];

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(LocalizationPatcher));
    }

    private static bool TryGetField<T>(string filePath, Dictionary<string, object> json, string fieldName, out T value)
    {
        value = default;
        if (!json.ContainsKey(fieldName))
        {
            BlenderAPI.LogError($"A localization file with path {filePath} had a missing {fieldName} field.");
            return false;
        }
        object val = json[fieldName];
        if (SimpleJson.SimpleJson.TryDeserializeObject(val.ToString(), out var obj))
        {
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
        foreach (string dirPath in Directory.GetDirectories(Paths.PluginPath))
        {
            string modName = Path.GetFileName(dirPath);
            string localizationPath = AssetHelper.LoadFile(modName, "localization.json");
            if (localizationPath != null)
            {
                string json = File.ReadAllText(localizationPath);
                var result = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, object>[]>(json);
                if (result != null)
                {
                    foreach (var jsonObject in result)
                    {
                        string key = null;
                        Localization.Categories category = Localization.Categories.NoCategory;
                        Dictionary<string, object> translations = null;
                        TranslationElement element = new();

                        if (TryGetField(localizationPath, jsonObject, "key", out string _key))
                            key = _key;
                        if (TryGetField(localizationPath, jsonObject, "category", out string _category))
                            category = (Localization.Categories)Enum.Parse(typeof(Localization.Categories), _category);
                        if (TryGetField(localizationPath, jsonObject, "translations", out Dictionary<string, object> _translations))
                        {
                            translations = _translations;
                        }

                        if (key != null && translations != null)
                        {
                            Localization.Translation[] results = new Localization.Translation[Enum.GetValues(typeof(Localization.Languages)).Length];
                            foreach (string lang in translations.Keys)
                            {
                                Localization.Languages language = (Localization.Languages)Enum.Parse(typeof(Localization.Languages), lang);
                                Localization.Translation translation = new();

                                if (TryGetField(localizationPath, translations, lang, out Dictionary<string, object> dict))
                                {
                                    if (TryGetField(localizationPath, dict, "text", out string _text))
                                    {
                                        translation.text = _text;
                                        results[(int)language] = translation;
                                    }
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
                            Elements.Add(element);
                        }
                    }
                }
            }
        }
        foreach (TranslationElement translation in Elements)
            Localization.Instance.m_TranslationElements.Add(translation);
    }

    private static int LargestId {
        get
        {
            return Localization.Instance.m_TranslationElements.Count;
        }
    }
}