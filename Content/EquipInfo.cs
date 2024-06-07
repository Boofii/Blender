namespace Blender.Content;

public class EquipInfo
{

    public string DisplayName { get; private set; }
    public string Subtext { get; private set; }
    public string Description { get; private set; }
    public string BundleName { get; private set; } = string.Empty;
    public string[] NormalIcons { get; private set; } = [];
    public string[] GreyIcons { get; private set; } = [];

    public EquipInfo SetDisplayName(string displayName)
    {
        this.DisplayName = displayName;
        return this;
    }

    public EquipInfo SetSubtext(string subtext)
    {
        this.Subtext = subtext;
        return this;
    }

    public EquipInfo SetDescription(string description)
    {
        this.Description = description;
        return this;
    }

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