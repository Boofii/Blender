using BepInEx;
using System.IO;

namespace Blender.Utility;

public class Identifier(string modName, string path)
{
    public string ModName { get; } = modName;
    public string Path { get; } = path;
    public string ModPath { get; private set; } = System.IO.Path.Combine(Paths.PluginPath, modName);
    public string ActualPath { get; private set; } = System.IO.Path.Combine(System.IO.Path.Combine(Paths.PluginPath, modName), path);

    public bool Validate()
    {
        if (string.IsNullOrEmpty(ModName))
        {
            BlenderAPI.LogError("Tried to get an identifier from a mod with an empty or null name.");
            return false;
        }
        else if (!Directory.Exists(ModPath))
        {
            BlenderAPI.LogError($"Tried to get an identifier from a mod that wasn't found on the plugins folder with name \"{ModName}\".");
            return false;
        }
        else if (string.IsNullOrEmpty(Path))
        {
            BlenderAPI.LogError($"Tried to get an identifier using a path that was empty or null for a mod with name \"{ModName}\".");
            return false;
        }
        else if (!File.Exists(ActualPath)) {
            BlenderAPI.LogError($"Tried to get an identifier from a path that didn't exist with path \"{ActualPath}\".");
            return false;
        }
        return true;
    }

    public Identifier Combine(string path)
    {
        return new Identifier(ModName, System.IO.Path.Combine(Path, path));
    }

    public override string ToString()
    {
        return $"{ModName}:{Path}";
    }
}