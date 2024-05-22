namespace CupAPI.Content;

public interface IEquipInfo
{
    public string DisplayName { get; }
    public string Subtext { get; }
    public string Description { get; }
    public string BundleName { get; }
    public string[] NormalIcons { get; }
    public string[] GreyIcons { get; }
}