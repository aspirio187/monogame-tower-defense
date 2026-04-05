using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_funny_game
{
    public class Enemy
    {
        public Vector2 Position;
        public float HP;
        public float MaxHP;
        public float Speed;
        public EnemyType Type;
        public int Reward;
        public int WaypointIndex;
        public bool ReachedEnd;
        public float SpeedMultiplier;
        public Texture2D Texture;

        private float _slowTimer;

        public bool IsAlive => HP > 0 && !ReachedEnd;

        public Enemy(EnemyType type, Vector2 spawnPosition, Texture2D texture)
        {
            Type = type;
            Position = spawnPosition;
            Texture = texture;
            MaxHP = EnemyStats.GetHP(type);
            HP = MaxHP;
            Speed = EnemyStats.GetSpeed(type);
            Reward = EnemyStats.GetReward(type);
            WaypointIndex = 1;
            SpeedMultiplier = 1.0f;
            ReachedEnd = false;
            _slowTimer = 0f;
        }

        public void Update(GameTime gameTime, List<Vector2> waypoints)
        {
            if (!IsAlive)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Decrement slow timer and reset multiplier when expired
            if (_slowTimer > 0f)
            {
                _slowTimer -= deltaTime;
                if (_slowTimer <= 0f)
                {
                    _slowTimer = 0f;
                    SpeedMultiplier = 1.0f;
                }
            }

            // Move toward current waypoint
            if (WaypointIndex >= waypoints.Count)
            {
                ReachedEnd = true;
                return;
            }

            Vector2 target = waypoints[WaypointIndex];
            Vector2 direction = target - Position;
            float distance = direction.Length();

            if (distance <= 2f)
            {
                // Arrived at waypoint, advance to next
                WaypointIndex++;
                if (WaypointIndex >= waypoints.Count)
                {
                    ReachedEnd = true;
                    return;
                }
            }
            else
            {
                // Normalize and move
                direction.Normalize();
                float moveAmount = Speed * SpeedMultiplier * deltaTime;
                // Don't overshoot the waypoint
                if (moveAmount > distance)
                    moveAmount = distance;
                Position += direction * moveAmount;
            }
        }

        public void TakeDamage(float damage)
        {
            HP -= damage;
        }

        public void ApplySlow(float factor, float duration)
        {
            SpeedMultiplier = factor;
            _slowTimer = duration;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (!IsAlive)
                return;

            // Draw enemy texture centered on Position (sprites are 32x32)
            int spriteSize = 32;
            Vector2 origin = new Vector2(spriteSize / 2f, spriteSize / 2f);
            spriteBatch.Draw(
                Texture,
                Position,
                null,
                Color.White,
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0f
            );

            // Draw HP bar above the enemy
            int barWidth = 32;
            int barHeight = 4;
            int barOffsetY = 4; // pixels above the sprite top

            // Bar position: centered above sprite
            float barX = Position.X - barWidth / 2f;
            float barY = Position.Y - spriteSize / 2f - barOffsetY - barHeight;

            // Background (dark red)
            spriteBatch.Draw(
                pixelTexture,
                new Rectangle((int)barX, (int)barY, barWidth, barHeight),
                Color.DarkRed
            );

            // Foreground (green) proportional to HP/MaxHP
            float hpRatio = MathHelper.Clamp(HP / MaxHP, 0f, 1f);
            int greenWidth = (int)(barWidth * hpRatio);
            if (greenWidth > 0)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)barX, (int)barY, greenWidth, barHeight),
                    Color.Green
                );
            }
        }
    }
}
