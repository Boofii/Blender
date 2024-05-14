using BepInEx.Logging;

namespace Blender;

internal class Logger
{

    private static ManualLogSource Instance = null;

    internal static void Initialize(ManualLogSource logger)
    {
        Instance = logger;
    }

    internal static void Info(string message)
    {
        Instance.LogInfo(message);
    }

    internal static void Warning(string message)
    {
        Instance.LogWarning(message);
    }

    internal static void Error(string message)
    {
        Instance.LogError(message);
    }
}