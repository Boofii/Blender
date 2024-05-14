using BepInEx;
using Blender.Utility;
using HarmonyLib;

namespace Blender;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class Initializer : BaseUnityPlugin {

    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake() {
        Blender.Logger.Initialize(this.Logger);
        Blender.Logger.Info($"Blender v{PluginInfo.PLUGIN_VERSION} was initialized.");
        CustomData.Initialize(Harmony);
    }
}