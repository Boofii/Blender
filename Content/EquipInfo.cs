namespace Blender.Content;

public class EquipInfo
{
    public string BundleName { get; private set; } = string.Empty;
    public string[] NormalIcons { get; private set; } = [];
    public string[] GreyIcons { get; private set; } = [];

    public EquipInfo SetBundleName(string bundleName)
    {
        this.BundleName = bundleName;
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

    public WeaponInfo AsWeaponInfo()
    {
        return this as WeaponInfo;
    }
}