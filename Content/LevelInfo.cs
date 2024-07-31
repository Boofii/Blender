using System;

namespace Blender.Content;

public class LevelInfo
{
    public Type LevelType { get; private set; }
    public string BundlePath { get; private set; }
    public string LevelName { get; private set; }
    public PlayerMode PlayerMode { get; private set; } = PlayerMode.Level;
    public Level.Type ActualType { get; private set; } = Level.Type.Battle;
    public Level.GoalTimes DefaultGoalTimes { get; private set; } = new Level.GoalTimes(120F, 120F, 120F);
    public Level.Spawns Spawns { get; private set; } = new Level.Spawns();
    public string ResourcesScene { get; private set; }
    public Action<Level> SetupAction { get; private set; }

    public LevelInfo(Type levelType, string bundlePath, string levelName)
    {
        this.LevelType = levelType;
        this.BundlePath = bundlePath;
        this.LevelName = levelName;
        this.ResourcesScene = GetResourcesScene(this.ActualType, this.PlayerMode);
    }

    public LevelInfo SetPlayerMode(PlayerMode playerMode)
    {
        this.PlayerMode = playerMode;
        this.ResourcesScene = GetResourcesScene(this.ActualType, this.PlayerMode);
        return this;
    }

    public LevelInfo SetActualType(Level.Type actualType)
    {
        this.ActualType = actualType;
        this.ResourcesScene = GetResourcesScene(this.ActualType, this.PlayerMode);
        return this;
    }

    public LevelInfo SetDefaultGoalTimes(Level.GoalTimes defaultGoalTimes)
    {
        this.DefaultGoalTimes = defaultGoalTimes;
        return this;
    }

    public LevelInfo SetSpawns(Level.Spawns spawns)
    {
        this.Spawns = spawns;
        return this;
    }

    public LevelInfo SetSetupAction(Action<Level> setupAction) {
        this.SetupAction += setupAction;
        return this;
    }

    private string GetResourcesScene(Level.Type type, PlayerMode playerMode)
    {
        if (type == Level.Type.Platforming)
            return "scene_level_platforming_1_1F";
        else
            if (playerMode == PlayerMode.Plane)
            return "scene_level_airplane";
        else
            return "scene_level_slime";
    }
}