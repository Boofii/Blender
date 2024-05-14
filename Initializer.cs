using BepInEx;
using Blender.Patching;
using Blender.Utility;
using HarmonyLib;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class Initializer : BaseUnityPlugin {

    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake() {
        Blender.Logger.Initialize(this.Logger);
        Logger.LogInfo($"Blender v{PluginInfo.PLUGIN_VERSION} was initialized.");
        CustomData.Initialize(Harmony);
        EnumPatcher.Initialize(Harmony);
    }
}