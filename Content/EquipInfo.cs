using Blender.Utility;

namespace Blender.Content;

public class EquipInfo
{
    public Identifier BundleId { get; private set; } = null;
    public string[] NormalIcons { get; private set; } = [];
    public string[] GreyIcons { get; private set; } = [];
    public int Cost { get; private set; } = 3;
    public ShopInfo ShopInfo { get; private set; } = null;

    public EquipInfo SetBundleId(Identifier bundleId)
    {
        this.BundleId = bundleId;
        return this;
    }

    public EquipInfo SetNormalIcons(string[] normalIcons)
    {
        this.NormalIcons = normalIcons;
        return this;
    }

    public EquipInfo SetGreyIcons(string[] greyIcons)
    {
        this.GreyIcons = greyIcons;
        return this;
    }

    public EquipInfo SetCost(int cost)
    {
        this.Cost = cost;
        return this;
    }

    public EquipInfo SetShopInfo(ShopInfo shopInfo)
    {
        this.ShopInfo = shopInfo;
        return this;
    }

    public WeaponInfo AsWeaponInfo()
    {
        return this as WeaponInfo;
    }
}