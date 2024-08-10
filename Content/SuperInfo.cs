using System;

namespace Blender.Content;

public class SuperInfo(Type superType, string bundlePath) : EquipInfo
{
    public Type SuperType { get; private set; } = superType;
    public string BundlePath { get; private set; } = bundlePath;
    public bool IsChaliceSuper = false;

    public SuperInfo SetChaliceSuper()
    {
        IsChaliceSuper = true;
        return this;
    }
}