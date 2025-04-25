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
        AtlasPath = atlasPath;
        return this;
    }

    public EquipInfo SetNormalIcons(string[] normalIcons)
    {
        NormalIcons = normalIcons;
        return this;
    }

    public EquipInfo SetGreyIcons(string[] greyIcons)
    {
        GreyIcons = greyIcons;
        return this;
    }

    public EquipInfo SetCost(int cost)
    {
        Cost = cost;
        return this;
    }

    public EquipInfo SetShopInfo(ShopInfo shopInfo)
    {
        ShopInfo = shopInfo;
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