using System;

namespace monogame_funny_game;

public enum TowerType
{
    Arrow,
    Cannon,
    Ice
}

public static class TowerStats
{
    public static int GetCost(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => 50,
            TowerType.Cannon => 100,
            TowerType.Ice => 75,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static float GetDamage(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => 10f,
            TowerType.Cannon => 40f,
            TowerType.Ice => 5f,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static float GetRange(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => 150f,
            TowerType.Cannon => 120f,
            TowerType.Ice => 130f,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static float GetFireRate(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => 2.0f,
            TowerType.Cannon => 0.5f,
            TowerType.Ice => 1.5f,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static float GetBulletSpeed(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => 400f,
            TowerType.Cannon => 250f,
            TowerType.Ice => 350f,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
