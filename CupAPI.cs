using BepInEx;
using BepInEx.Logging;
using CupAPI.Content;
using CupAPI.Util;
using HarmonyLib;
using UnityEngine;

namespace CupAPI {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class CupAPI : BaseUnityPlugin {

        public static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        public static new ManualLogSource Logger;

        private void Awake() {
            Harmony.PatchAll(); Logger = base.Logger;
            Logger.LogInfo($"CupAPI v{PluginInfo.PLUGIN_VERSION} was initialized!");
            CustomData.Initialize();

            Registries.Charms.Register("charm_god", new GodCharm());
            AssetHelper.CacheAssets<Sprite>("equip_menu");
        }
    }
}