using Blender.Content;
using Blender.Utility;
using HarmonyLib;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(ShopScenePlayer))]
internal class ShopPatcher
{
    [HarmonyPatch(nameof(ShopScenePlayer.Awake))]
    [HarmonyPrefix]
    private static void Patch_Awake(ShopScenePlayer __instance)
    {
        foreach (EquipInfo info in EquipRegistries.Charms.GetValues())
        {
            ShopInfo shopInfo = info.ShopInfo;
            if (shopInfo != null)
            {
                AssetHelper.LoadAsset<GameObject>(info.BundleId, shopInfo.ItemName, (item) =>
                {
                    ShopSceneItem itemComponent = item.GetComponent<ShopSceneItem>();
                    itemComponent.buyAnimation = __instance.charmItemPrefabs[0].buyAnimation;
                    __instance.charmItemPrefabs = __instance.charmItemPrefabs.AddToArray(itemComponent);
                });
            }
        }
        foreach (EquipInfo info in EquipRegistries.Weapons.GetValues())
        {
            ShopInfo shopInfo = info.ShopInfo;
            if (shopInfo != null)
            {
                AssetHelper.LoadAsset<GameObject>(info.BundleId, shopInfo.ItemName, (item) =>
                {
                    ShopSceneItem itemComponent = item.GetComponent<ShopSceneItem>();
                    itemComponent.buyAnimation = __instance.weaponItemPrefabs[0].buyAnimation;
                    __instance.weaponItemPrefabs = __instance.weaponItemPrefabs.AddToArray(itemComponent);
                });
            }
        }
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(ShopPatcher));
    }
}