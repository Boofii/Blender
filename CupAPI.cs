using BepInEx;
using BepInEx.Logging;
using CupAPI.Content;
using CupAPI.Util;
using CupAPI.Utility;
using HarmonyLib;
using System;

namespace CupAPI {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class CupAPI : BaseUnityPlugin {

        public static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        public static new ManualLogSource Logger;

        private void Awake() {
            Harmony.PatchAll(); Logger = base.Logger;
            Logger.LogInfo($"CupAPI v{PluginInfo.PLUGIN_VERSION} was initialized!");
            CustomData.Initialize();

            EnumManager.Register<Charm, ICharm>();
            if (EnumManager.TryGetRegistry<Charm>(out var enumRegistry) && EnumManager.TryGetRegistry<Charm, ICharm>(out var registry)) {
                enumRegistry.Register("charm_god");
                registry.Register("charm_god", new GodCharm());
            }
        }
    }
}