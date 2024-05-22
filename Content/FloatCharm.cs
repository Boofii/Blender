using CupAPI.Content;

namespace Blender.Content;

public class FloatCharm : IEquipInfo
{
    public string DisplayName => "Float";

    public string Subtext => "Subtext";

    public string Description => "Description";

    public string BundleName => "blender_content";

    public string[] NormalIcons => ["float0"];

    public string[] GreyIcons => [];
}