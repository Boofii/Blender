namespace Blender.Testing;

public class SparkEffect : WeaponSparkEffect
{
    public override void Awake()
    {
        base.Awake();
        randomRotation = true;
        randomMirrorX = true;
        randomMirrorY = true;
    }
}