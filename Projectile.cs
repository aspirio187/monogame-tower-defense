using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace monogame_funny_game;

public class Projectile
{
    public Vector2 Position;
    public Enemy Target;
    public float Speed;
    public float Damage;
    public bool IsIce;
    public float SlowFactor;
    public float SlowDuration;
    public bool IsActive;

    public Projectile(Vector2 startPosition, Enemy target, float speed, float damage, bool isIce)
    {
        Position = startPosition;
        Target = target;
        Speed = speed;
        Damage = damage;
        IsIce = isIce;
        SlowFactor = isIce ? 0.5f : 1.0f;
        SlowDuration = isIce ? 2.0f : 0f;
        IsActive = true;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        if (Target == null || !Target.IsAlive)
        {
            IsActive = false;
            return;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 direction = Target.Position - Position;
        float distance = direction.Length();

        if (distance < 5f)
        {
            Target.TakeDamage(Damage);
            if (IsIce)
                Target.ApplySlow(SlowFactor, SlowDuration);
            IsActive = false;
            return;
        }

        direction.Normalize();
        Position += direction * Speed * deltaTime;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        if (!IsActive)
            return;

        // Projectile sprite is 8x8, draw centered on Position
        Vector2 origin = new Vector2(4f, 4f);
        spriteBatch.Draw(texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
}
