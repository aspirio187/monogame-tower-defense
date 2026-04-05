using System;

namespace monogame_funny_game
{
    public enum EnemyType
    {
        Goblin,
        Orc,
        Wolf,
        Troll
    }

    public static class EnemyStats
    {
        public static float GetHP(EnemyType type)
        {
            return type switch
            {
                EnemyType.Goblin => 30f,
                EnemyType.Orc => 80f,
                EnemyType.Wolf => 20f,
                EnemyType.Troll => 200f,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static float GetSpeed(EnemyType type)
        {
            return type switch
            {
                EnemyType.Goblin => 60f,
                EnemyType.Orc => 40f,
                EnemyType.Wolf => 100f,
                EnemyType.Troll => 30f,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static int GetReward(EnemyType type)
        {
            return type switch
            {
                EnemyType.Goblin => 10,
                EnemyType.Orc => 20,
                EnemyType.Wolf => 15,
                EnemyType.Troll => 50,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
