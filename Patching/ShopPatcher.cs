using Blender.Content;
using HarmonyLib;
using UnityEngine;

namespace Blender.Patching;

[HarmonyPatch(typeof(ShopScenePlayer))]
internal static class ShopPatcher
{
    [HarmonyPatch(nameof(ShopScenePlayer.Awake))]
    [HarmonyPrefix]
    private static void Patch_Awake(ShopScenePlayer __instance)
    {
        foreach (string bundlePath in EquipRegistries.ProcessedItemBundles)
        {
            if (AssetLoader<Object[]>.Instance.tryGetAsset(bundlePath, out var items))
            {
                foreach (Object item in items)
                {
                    if (item is not GameObject itemObj)
                        return;

                    ShopSceneItem itemComponent = itemObj.GetComponent<ShopSceneItem>();
                    if (itemComponent != null)
                    {
                        switch (EquipRegistries.ProcessedItems[item.name])
                        {
                            case ItemType.Charm:
                                itemComponent.buyAnimation = __instance.charmItemPrefabs[0].buyAnimation;
                                __instance.charmItemPrefabs = __instance.charmItemPrefabs.AddToArray(itemComponent);
                                break;
                            default:
                                itemComponent.buyAnimation = __instance.weaponItemPrefabs[0].buyAnimation;
                                __instance.weaponItemPrefabs = __instance.weaponItemPrefabs.AddToArray(itemComponent);
                                break;
                        }
                    }
                }
            }
        }
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(ShopPatcher));
    }
}