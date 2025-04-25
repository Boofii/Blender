using UnityEngine;

namespace Blender.Content;

public class ShopInfo(string bundlePath)
{
    public string BundlePath { get; private set; } = bundlePath;
    public ItemType ItemType { get; private set; } = ItemType.Charm;
    public string Charm { get; private set; } = "None";
    public string Weapon { get; private set; } = "None";
    public string Super { get; private set; } = "None";
    public Vector3 OriginalShadowScale { get; private set; } = Vector3.one;

    public ShopInfo SetItemType(ItemType itemType)
    {
        ItemType = itemType;
        return this;
    }

    public ShopInfo SetCharm(string charm)
    {
        Charm = charm;
        return this;
    }

    public ShopInfo SetWeapon(string weapon)
    {
        Weapon = weapon;
        return this;
    }

    public ShopInfo SetSuper(string super)
    {
        Super = super;
        return this;
    }

    public ShopInfo SetOriginalShadowScale(Vector3 originalShadowScale)
    {
        OriginalShadowScale = originalShadowScale;
        return this;
    }
}