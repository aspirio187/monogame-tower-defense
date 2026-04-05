using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_funny_game;

public class Tower
{
    public TowerType Type;
    public int Col;
    public int Row;
    public Vector2 Position;
    public float Range;
    public float Damage;
    public float FireRate;
    public float BulletSpeed;
    public float FireCooldown;
    public Texture2D Texture;

    public Tower(TowerType type, int col, int row, Texture2D texture)
    {
        Type = type;
        Col = col;
        Row = row;
        Position = new Vector2(col * 64 + 32, row * 64 + 32);
        Range = TowerStats.GetRange(type);
        Damage = TowerStats.GetDamage(type);
        FireRate = TowerStats.GetFireRate(type);
        BulletSpeed = TowerStats.GetBulletSpeed(type);
        FireCooldown = 0f;
        Texture = texture;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Tower sprites are 48x48, draw centered on Position
        Vector2 origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        spriteBatch.Draw(Texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    public void Update(GameTime gameTime, List<Enemy> enemies, List<Projectile> projectiles)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        FireCooldown -= deltaTime;
        if (FireCooldown > 0)
            return;

        // Find target: enemy furthest along path (highest WaypointIndex) that is alive and in range
        Enemy bestTarget = null;
        int bestWaypointIndex = -1;
        float bestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
                continue;

            float dist = Vector2.Distance(Position, enemy.Position);
            if (dist > Range)
                continue;

            // Prefer highest WaypointIndex (furthest along path)
            // For same WaypointIndex, prefer closest to tower (tiebreaker)
            if (enemy.WaypointIndex > bestWaypointIndex ||
                (enemy.WaypointIndex == bestWaypointIndex && dist < bestDistance))
            {
                bestTarget = enemy;
                bestWaypointIndex = enemy.WaypointIndex;
                bestDistance = dist;
            }
        }

        if (bestTarget == null)
            return;

        // Fire projectile
        var projectile = new Projectile(Position, bestTarget, BulletSpeed, Damage, Type == TowerType.Ice);
        projectiles.Add(projectile);

        FireCooldown = 1f / FireRate;
    }
}
