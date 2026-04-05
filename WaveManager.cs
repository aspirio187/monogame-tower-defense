using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace monogame_funny_game
{
    public struct WaveDefinition
    {
        public List<EnemyType> Enemies;
        public float SpawnInterval;
    }

    public class WaveManager
    {
        private List<WaveDefinition> _waves;
        private int _enemiesSpawned;
        private float _spawnTimer;
        private float _pauseTimer;
        private bool _isPaused;
        public int CurrentWave;
        public bool AllWavesComplete;
        private Dictionary<EnemyType, Texture2D> _textures;

        public WaveManager(Dictionary<EnemyType, Texture2D> enemyTextures)
        {
            _textures = enemyTextures;
            _waves = new List<WaveDefinition>();

            // Wave 1: 5 Goblin, 1.5s
            _waves.Add(BuildWave(1.5f,
                (EnemyType.Goblin, 5)));

            // Wave 2: 8 Goblin, 1.2s
            _waves.Add(BuildWave(1.2f,
                (EnemyType.Goblin, 8)));

            // Wave 3: 5 Goblin + 3 Orc, 1.2s
            _waves.Add(BuildWave(1.2f,
                (EnemyType.Goblin, 5),
                (EnemyType.Orc, 3)));

            // Wave 4: 4 Wolf + 4 Goblin, 1.0s
            _waves.Add(BuildWave(1.0f,
                (EnemyType.Wolf, 4),
                (EnemyType.Goblin, 4)));

            // Wave 5: 6 Orc + 4 Wolf, 1.0s
            _waves.Add(BuildWave(1.0f,
                (EnemyType.Orc, 6),
                (EnemyType.Wolf, 4)));

            // Wave 6: 10 Goblin + 5 Orc, 0.8s
            _waves.Add(BuildWave(0.8f,
                (EnemyType.Goblin, 10),
                (EnemyType.Orc, 5)));

            // Wave 7: 8 Wolf + 6 Orc, 0.8s
            _waves.Add(BuildWave(0.8f,
                (EnemyType.Wolf, 8),
                (EnemyType.Orc, 6)));

            // Wave 8: 10 Orc + 5 Wolf + 1 Troll, 0.8s
            _waves.Add(BuildWave(0.8f,
                (EnemyType.Orc, 10),
                (EnemyType.Wolf, 5),
                (EnemyType.Troll, 1)));

            // Wave 9: 8 Orc + 8 Wolf + 2 Troll, 0.7s
            _waves.Add(BuildWave(0.7f,
                (EnemyType.Orc, 8),
                (EnemyType.Wolf, 8),
                (EnemyType.Troll, 2)));

            // Wave 10: 10 Orc + 10 Wolf + 3 Troll, 0.6s
            _waves.Add(BuildWave(0.6f,
                (EnemyType.Orc, 10),
                (EnemyType.Wolf, 10),
                (EnemyType.Troll, 3)));

            Reset();
        }

        private WaveDefinition BuildWave(float interval, params (EnemyType type, int count)[] groups)
        {
            var enemies = new List<EnemyType>();
            foreach (var (type, count) in groups)
            {
                for (int i = 0; i < count; i++)
                    enemies.Add(type);
            }
            return new WaveDefinition { Enemies = enemies, SpawnInterval = interval };
        }

        public void Reset()
        {
            CurrentWave = 0;
            _enemiesSpawned = 0;
            _spawnTimer = 0;
            _pauseTimer = 5f;
            _isPaused = true;
            AllWavesComplete = false;
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, List<Vector2> waypoints)
        {
            if (AllWavesComplete)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isPaused)
            {
                _pauseTimer -= dt;
                if (_pauseTimer <= 0)
                {
                    _isPaused = false;
                    _spawnTimer = 0;
                }
                return;
            }

            WaveDefinition currentWaveDef = _waves[CurrentWave];

            if (_enemiesSpawned >= currentWaveDef.Enemies.Count)
            {
                // All enemies in current wave have been spawned
                // Check if all spawned enemies are dead or reached end
                bool anyAlive = false;
                foreach (var enemy in enemies)
                {
                    if (enemy.IsAlive)
                    {
                        anyAlive = true;
                        break;
                    }
                }

                if (!anyAlive)
                {
                    CurrentWave++;
                    _enemiesSpawned = 0;
                    _pauseTimer = 5f;
                    _isPaused = true;

                    if (CurrentWave >= _waves.Count)
                    {
                        AllWavesComplete = true;
                    }
                }
                return;
            }

            _spawnTimer -= dt;

            if (_spawnTimer <= 0)
            {
                EnemyType type = currentWaveDef.Enemies[_enemiesSpawned];
                enemies.Add(new Enemy(type, waypoints[0], _textures[type]));
                _enemiesSpawned++;
                _spawnTimer = currentWaveDef.SpawnInterval;
            }
        }
    }
}
