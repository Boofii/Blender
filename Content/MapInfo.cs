using System;

namespace Blender.Content;

public class MapInfo(string bundlePath, string firstNode)
{
    public string BundlePath { get; private set; } = bundlePath;
    public string FirstNode { get; private set; } = firstNode;
    public Map.Camera CameraProperties { get; private set; } = new();
    public Action<Map> SetupAction { get; private set; }
    
    public MapInfo SetCameraProperties(Map.Camera properties)
    {
        this.CameraProperties = properties;
        return this;
    }

    public MapInfo SetSetupAction(Action<Map> setupAction)
    {
        SetupAction = setupAction;
        return this;
    }
}