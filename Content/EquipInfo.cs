namespace Blender.Content;

public class EquipInfo
{
    public string AtlasPath = null;
    public string[] NormalIcons { get; private set; } = [];
    public string[] GreyIcons { get; private set; } = [];
    public int Cost { get; private set; } = 3;
    public ShopInfo ShopInfo { get; private set; } = null;

    public EquipInfo SetAtlasPath(string atlasPath)
    {
        this.AtlasPath = atlasPath;
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

    public SuperInfo AsSuperInfo()
    {
        return this as SuperInfo;
    }
}