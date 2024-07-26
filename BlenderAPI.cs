using BepInEx;
using BepInEx.Logging;
using Blender.Content;
using Blender.Patching;
using Blender.Utility;
using HarmonyLib;
using System.IO;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class BlenderAPI : BaseUnityPlugin
{
    public static bool HasDLC { get; private set; } = false;
    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
    private static new ManualLogSource Logger = null;

    private void Awake()
    {
        base.Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was loaded.");
        BlenderAPI.Logger = base.Logger;

        string dlcBundlePath = Path.Combine(Paths.GameRootPath, "Cuphead_Data\\StreamingAssets\\AssetBundles\\atlas_world_dlc");
        if (File.Exists(dlcBundlePath))
            HasDLC = true;

        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
        EquipMenuPatcher.Initialize(Harmony);
        PropertiesPatcher.Initialize(Harmony);
        PrefabsPatcher.Initialize(Harmony);
        LocalizationPatcher.Initialize(Harmony);
        ShopPatcher.Initialize(Harmony);
        AudioPatcher.Initialize(Harmony);
        AssetHelper.Initialize(Harmony);
        EquipRegistries.Initialize();

        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /*private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "scene_slot_select")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StartCoroutine(GetLevelResources());
        }
    }

    private static IEnumerator GetLevelResources()
    {
        var request1 = SceneManager.LoadSceneAsync("scene_level_slime");
        while (!request1.isDone)
            yield return null;
        SlimeLevel level1 = FindObjectOfType<SlimeLevel>();
        LevelResources resources1 = level1.LevelResources;
        AssetHelper.AddPrefab(resources1.gameObject, true);

        var request2 = SceneManager.LoadSceneAsync("scene_level_flying_blimp");
        while (!request2.isDone)
            yield return null;
        FlyingBlimpLevel level2 = FindObjectOfType<FlyingBlimpLevel>();
        LevelResources resources2 = level2.LevelResources;
        AssetHelper.AddPrefab(resources2.gameObject, true);

        SceneManager.LoadScene("scene_slot_select");
    }*/

    internal static void LogInfo(string message)
    {
        Logger.LogInfo(message);
    }

    internal static void LogWarning(string message)
    {
        Logger.LogWarning(message);
    }

    internal static void LogError(string message)
    {
        Logger.LogError(message);
    }
}