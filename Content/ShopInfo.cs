using UnityEngine;

namespace Blender.Content;

public class ShopInfo(string bundlePath, string itemName)
{
    public string BundlePath { get; private set; } = bundlePath;
    public string ItemName { get; private set; } = itemName;
    public ItemType ItemType { get; private set; } = ItemType.Charm;
    public string Charm { get; private set; } = "None";
    public string Weapon { get; private set; } = "None";
    public string Super { get; private set; } = "None";
    public Vector3 OriginalShadowScale { get; private set; } = Vector3.one;

    public ShopInfo SetItemType(ItemType itemType)
    {
        this.ItemType = itemType;
        return this;
    }

    public ShopInfo SetCharm(string charm)
    {
        this.Charm = charm;
        return this;
    }

    public ShopInfo SetWeapon(string weapon)
    {
        this.Weapon = weapon;
        return this;
    }

    public ShopInfo SetSuper(string super)
    {
        this.Super = super;
        return this;
    }

    public ShopInfo SetOriginalShadowScale(Vector3 originalShadowScale)
    {
        this.OriginalShadowScale = originalShadowScale;
        return this;
    }
}