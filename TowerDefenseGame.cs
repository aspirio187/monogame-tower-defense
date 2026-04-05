using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_funny_game;

public enum GameScreen
{
    Menu,
    Playing,
    GameOver,
    Victory
}

public class TowerDefenseGame : Game
{
    // Core MonoGame
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _hudFont;
    private Texture2D _pixelTexture;

    // Tile textures
    private Texture2D _grassTexture;
    private Texture2D _pathTexture;

    // Tower textures
    private Texture2D _towerArrowTexture;
    private Texture2D _towerCannonTexture;
    private Texture2D _towerIceTexture;

    // Enemy textures
    private Texture2D _enemyGoblinTexture;
    private Texture2D _enemyOrcTexture;
    private Texture2D _enemyWolfTexture;
    private Texture2D _enemyTrollTexture;

    // Projectile texture
    private Texture2D _projectileTexture;

    // Texture dictionaries
    private Dictionary<EnemyType, Texture2D> _enemyTextures;
    private Dictionary<TowerType, Texture2D> _towerTextures;

    // Game objects
    private Map _map;
    private List<Enemy> _enemies;
    private List<Tower> _towers;
    private List<Projectile> _projectiles;
    private WaveManager _waveManager;
    private GameState _gameState;

    // UI / flow state
    private TowerType _selectedTower = TowerType.Arrow;
    private GameScreen _currentScreen = GameScreen.Menu;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private float _gameOverTimer;

    public TowerDefenseGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load textures
        _grassTexture = Content.Load<Texture2D>("grass");
        _pathTexture = Content.Load<Texture2D>("path");

        _towerArrowTexture = Content.Load<Texture2D>("tower_arrow");
        _towerCannonTexture = Content.Load<Texture2D>("tower_cannon");
        _towerIceTexture = Content.Load<Texture2D>("tower_ice");

        _enemyGoblinTexture = Content.Load<Texture2D>("enemy_goblin");
        _enemyOrcTexture = Content.Load<Texture2D>("enemy_orc");
        _enemyWolfTexture = Content.Load<Texture2D>("enemy_wolf");
        _enemyTrollTexture = Content.Load<Texture2D>("enemy_troll");

        _projectileTexture = Content.Load<Texture2D>("projectile");

        // Load font
        _hudFont = Content.Load<SpriteFont>("hud_font");

        // Create 1x1 white pixel texture for drawing primitives
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Build dictionaries
        _enemyTextures = new Dictionary<EnemyType, Texture2D>
        {
            { EnemyType.Goblin, _enemyGoblinTexture },
            { EnemyType.Orc, _enemyOrcTexture },
            { EnemyType.Wolf, _enemyWolfTexture },
            { EnemyType.Troll, _enemyTrollTexture }
        };

        _towerTextures = new Dictionary<TowerType, Texture2D>
        {
            { TowerType.Arrow, _towerArrowTexture },
            { TowerType.Cannon, _towerCannonTexture },
            { TowerType.Ice, _towerIceTexture }
        };

        StartNewGame();
    }

    private void StartNewGame()
    {
        _map = new Map();
        _enemies = new List<Enemy>();
        _towers = new List<Tower>();
        _projectiles = new List<Projectile>();
        _gameState = new GameState();
        _waveManager = new WaveManager(_enemyTextures);
        _selectedTower = TowerType.Arrow;
        _currentScreen = GameScreen.Playing;
    }

    protected override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();

        switch (_currentScreen)
        {
            case GameScreen.Menu:
                UpdateMenu(mouseState);
                break;

            case GameScreen.Playing:
                UpdatePlaying(gameTime, mouseState, keyboardState);
                break;

            case GameScreen.GameOver:
            case GameScreen.Victory:
                UpdateEndScreen(gameTime, mouseState, keyboardState);
                break;
        }

        _previousMouse = mouseState;
        _previousKeyboard = keyboardState;

        base.Update(gameTime);
    }

    private bool IsMouseClicked(MouseState current)
    {
        return current.LeftButton == ButtonState.Pressed &&
               _previousMouse.LeftButton == ButtonState.Released;
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void UpdateMenu(MouseState mouseState)
    {
        if (!IsMouseClicked(mouseState))
            return;

        int screenW = _graphics.PreferredBackBufferWidth;
        int screenH = _graphics.PreferredBackBufferHeight;

        // "Play" button area
        string playText = "Play";
        Vector2 playSize = _hudFont.MeasureString(playText);
        int playX = (int)(screenW / 2 - playSize.X / 2) - 20;
        int playY = (int)(screenH / 2 - 10);
        Rectangle playRect = new Rectangle(playX, playY, (int)playSize.X + 40, (int)playSize.Y + 20);

        // "Quit" button area
        string quitText = "Quit";
        Vector2 quitSize = _hudFont.MeasureString(quitText);
        int quitX = (int)(screenW / 2 - quitSize.X / 2) - 20;
        int quitY = playY + (int)playSize.Y + 50;
        Rectangle quitRect = new Rectangle(quitX, quitY, (int)quitSize.X + 40, (int)quitSize.Y + 20);

        Point click = new Point(mouseState.X, mouseState.Y);

        if (playRect.Contains(click))
            StartNewGame();
        else if (quitRect.Contains(click))
            Exit();
    }

    private void UpdatePlaying(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // --- Keyboard input ---
        if (IsKeyPressed(keyboardState, Keys.D1))
            _selectedTower = TowerType.Arrow;
        if (IsKeyPressed(keyboardState, Keys.D2))
            _selectedTower = TowerType.Cannon;
        if (IsKeyPressed(keyboardState, Keys.D3))
            _selectedTower = TowerType.Ice;
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _currentScreen = GameScreen.Menu;
            return;
        }
        if (IsKeyPressed(keyboardState, Keys.R))
        {
            _map.Generate();
            _enemies.Clear();
            _towers.Clear();
            _projectiles.Clear();
            _waveManager.Reset();
            _gameState.Reset();
            return;
        }

        // --- Mouse click: place tower ---
        if (IsMouseClicked(mouseState))
        {
            int col = mouseState.X / Map.TileSize;
            int row = mouseState.Y / Map.TileSize;

            if (row < Map.Rows && col >= 0 && col < Map.Cols &&
                _map.IsBuildable(col, row) &&
                _gameState.TryPurchase(TowerStats.GetCost(_selectedTower)))
            {
                var tower = new Tower(_selectedTower, col, row, _towerTextures[_selectedTower]);
                _towers.Add(tower);
                _map.Occupied[col, row] = true;
            }
        }

        // --- Update wave manager (spawns enemies) ---
        _waveManager.Update(gameTime, _enemies, _map.Waypoints);

        // --- Update enemies ---
        foreach (var enemy in _enemies)
        {
            enemy.Update(gameTime, _map.Waypoints);
        }

        // --- Update towers (they fire projectiles) ---
        foreach (var tower in _towers)
        {
            tower.Update(gameTime, _enemies, _projectiles);
        }

        // --- Update projectiles ---
        foreach (var projectile in _projectiles)
        {
            projectile.Update(gameTime);
        }

        // --- Process enemy deaths / reached-end, then remove ---
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (!enemy.IsAlive)
            {
                if (enemy.HP <= 0)
                    _gameState.EnemyKilled(enemy.Reward);
                else if (enemy.ReachedEnd)
                    _gameState.EnemyReachedEnd();

                _enemies.RemoveAt(i);
            }
        }

        // --- Remove inactive projectiles ---
        _projectiles.RemoveAll(p => !p.IsActive);

        // --- Check win / lose ---
        if (_gameState.GameOver)
        {
            _currentScreen = GameScreen.GameOver;
            _gameOverTimer = 3f;
        }
        else if (_waveManager.AllWavesComplete && _enemies.Count == 0)
        {
            _gameState.GameWon = true;
            _currentScreen = GameScreen.Victory;
            _gameOverTimer = 3f;
        }
    }

    private void UpdateEndScreen(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _gameOverTimer -= deltaTime;

        if (_gameOverTimer <= 0f || IsMouseClicked(mouseState) ||
            IsKeyPressed(keyboardState, Keys.Enter) ||
            IsKeyPressed(keyboardState, Keys.Escape))
        {
            _currentScreen = GameScreen.Menu;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        switch (_currentScreen)
        {
            case GameScreen.Menu:
                DrawMenu();
                break;

            case GameScreen.Playing:
                DrawPlaying();
                break;

            case GameScreen.GameOver:
                DrawEndScreen("GAME OVER", Color.Red);
                break;

            case GameScreen.Victory:
                DrawEndScreen("VICTORY!", Color.Gold);
                break;
        }

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        GraphicsDevice.Clear(new Color(20, 20, 40));

        int screenW = _graphics.PreferredBackBufferWidth;
        int screenH = _graphics.PreferredBackBufferHeight;

        _spriteBatch.Begin();

        // Title
        string title = "Tower Defense";
        Vector2 titleSize = _hudFont.MeasureString(title);
        Vector2 titlePos = new Vector2(screenW / 2 - titleSize.X / 2, screenH / 4 - titleSize.Y / 2);
        // Draw title with a simple scale-up by drawing twice (shadow + text)
        _spriteBatch.DrawString(_hudFont, title, titlePos + new Vector2(2, 2), Color.Black);
        _spriteBatch.DrawString(_hudFont, title, titlePos, Color.White);

        // Play button
        string playText = "Play";
        Vector2 playSize = _hudFont.MeasureString(playText);
        int playX = (int)(screenW / 2 - playSize.X / 2) - 20;
        int playY = (int)(screenH / 2 - 10);
        Rectangle playRect = new Rectangle(playX, playY, (int)playSize.X + 40, (int)playSize.Y + 20);

        // Button background
        _spriteBatch.Draw(_pixelTexture, playRect, new Color(60, 60, 120));
        // Button border
        DrawRectBorder(playRect, Color.White, 2);
        // Button text
        _spriteBatch.DrawString(_hudFont, playText,
            new Vector2(playRect.X + (playRect.Width - playSize.X) / 2,
                        playRect.Y + (playRect.Height - playSize.Y) / 2),
            Color.White);

        // Quit button
        string quitText = "Quit";
        Vector2 quitSize = _hudFont.MeasureString(quitText);
        int quitX = (int)(screenW / 2 - quitSize.X / 2) - 20;
        int quitY = playY + (int)playSize.Y + 50;
        Rectangle quitRect = new Rectangle(quitX, quitY, (int)quitSize.X + 40, (int)quitSize.Y + 20);

        _spriteBatch.Draw(_pixelTexture, quitRect, new Color(60, 60, 120));
        DrawRectBorder(quitRect, Color.White, 2);
        _spriteBatch.DrawString(_hudFont, quitText,
            new Vector2(quitRect.X + (quitRect.Width - quitSize.X) / 2,
                        quitRect.Y + (quitRect.Height - quitSize.Y) / 2),
            Color.White);

        // Controls hint
        string controls = "Controls: [1/2/3] Select Tower  [Click] Place  [R] Restart  [ESC] Menu";
        Vector2 controlsSize = _hudFont.MeasureString(controls);
        _spriteBatch.DrawString(_hudFont, controls,
            new Vector2(screenW / 2 - controlsSize.X / 2, screenH - 60), Color.Gray);

        _spriteBatch.End();
    }

    private void DrawPlaying()
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw map
        _map.Draw(_spriteBatch, _grassTexture, _pathTexture);

        // Draw placement preview
        DrawPlacementPreview();

        // Draw towers
        foreach (var tower in _towers)
        {
            tower.Draw(_spriteBatch);
        }

        // Draw enemies
        foreach (var enemy in _enemies)
        {
            enemy.Draw(_spriteBatch, _pixelTexture);
        }

        // Draw projectiles
        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteBatch, _projectileTexture);
        }

        // Draw HUD
        DrawHUD();

        _spriteBatch.End();
    }

    private void DrawPlacementPreview()
    {
        var mouseState = Mouse.GetState();
        int col = mouseState.X / Map.TileSize;
        int row = mouseState.Y / Map.TileSize;

        // Only show preview in play area
        if (col < 0 || col >= Map.Cols || row < 0 || row >= Map.Rows)
            return;

        bool buildable = _map.IsBuildable(col, row);
        bool affordable = _gameState.Currency >= TowerStats.GetCost(_selectedTower);
        bool canPlace = buildable && affordable;

        Color tintColor = canPlace ? new Color(0, 255, 0, 100) : new Color(255, 0, 0, 100);

        // Draw tinted tile overlay
        Rectangle tileRect = new Rectangle(col * Map.TileSize, row * Map.TileSize, Map.TileSize, Map.TileSize);
        _spriteBatch.Draw(_pixelTexture, tileRect, tintColor);

        // Draw tower preview (semi-transparent)
        Texture2D previewTexture = _towerTextures[_selectedTower];
        Vector2 towerCenter = new Vector2(col * Map.TileSize + Map.TileSize / 2,
                                          row * Map.TileSize + Map.TileSize / 2);
        Vector2 origin = new Vector2(previewTexture.Width / 2f, previewTexture.Height / 2f);
        Color previewColor = canPlace ? new Color(255, 255, 255, 150) : new Color(255, 100, 100, 150);
        _spriteBatch.Draw(previewTexture, towerCenter, null, previewColor, 0f, origin, 1f, SpriteEffects.None, 0f);

        // Draw range circle
        float range = TowerStats.GetRange(_selectedTower);
        Color circleColor = canPlace ? new Color(0, 200, 0, 180) : new Color(200, 0, 0, 180);
        DrawCircle(_spriteBatch, towerCenter, range, circleColor);
    }

    private void DrawHUD()
    {
        int hudY = Map.Rows * Map.TileSize; // 640
        int hudHeight = 80;
        int screenW = _graphics.PreferredBackBufferWidth;

        // HUD background
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(0, hudY, screenW, hudHeight),
            new Color(30, 30, 50));

        // Top line: wave, lives, currency
        string infoText = $"Wave: {_waveManager.CurrentWave + 1}/10   Lives: {_gameState.Lives}   Gold: {_gameState.Currency}";
        _spriteBatch.DrawString(_hudFont, infoText, new Vector2(16, hudY + 8), Color.White);

        // Bottom line: tower selection
        int towerLineY = hudY + 42;
        float xPos = 16;

        DrawTowerOption(TowerType.Arrow, "1", ref xPos, towerLineY);
        DrawTowerOption(TowerType.Cannon, "2", ref xPos, towerLineY);
        DrawTowerOption(TowerType.Ice, "3", ref xPos, towerLineY);

        // Right-side hint
        string hint = "[R] Restart  [ESC] Menu";
        Vector2 hintSize = _hudFont.MeasureString(hint);
        _spriteBatch.DrawString(_hudFont, hint,
            new Vector2(screenW - hintSize.X - 16, hudY + 8), Color.Gray);
    }

    private void DrawTowerOption(TowerType type, string key, ref float xPos, int y)
    {
        int cost = TowerStats.GetCost(type);
        bool selected = _selectedTower == type;
        bool affordable = _gameState.Currency >= cost;

        string text = $"[{key}]{type} ${cost}";
        Vector2 textSize = _hudFont.MeasureString(text);

        // Highlight background if selected
        if (selected)
        {
            Rectangle bg = new Rectangle((int)xPos - 4, y - 2, (int)textSize.X + 8, (int)textSize.Y + 4);
            _spriteBatch.Draw(_pixelTexture, bg, new Color(80, 80, 140));
        }

        Color textColor = selected ? Color.Yellow : (affordable ? Color.LightGray : Color.DarkGray);
        _spriteBatch.DrawString(_hudFont, text, new Vector2(xPos, y), textColor);

        xPos += textSize.X + 24;
    }

    private void DrawEndScreen(string message, Color color)
    {
        GraphicsDevice.Clear(new Color(20, 20, 40));

        int screenW = _graphics.PreferredBackBufferWidth;
        int screenH = _graphics.PreferredBackBufferHeight;

        _spriteBatch.Begin();

        // Main message
        Vector2 messageSize = _hudFont.MeasureString(message);
        Vector2 messagePos = new Vector2(screenW / 2 - messageSize.X / 2,
                                         screenH / 2 - messageSize.Y / 2 - 30);
        // Shadow
        _spriteBatch.DrawString(_hudFont, message, messagePos + new Vector2(3, 3), Color.Black);
        _spriteBatch.DrawString(_hudFont, message, messagePos, color);

        // Sub-text
        string sub = "Click or press Enter to continue...";
        Vector2 subSize = _hudFont.MeasureString(sub);
        float alpha = 0.5f + 0.5f * (float)Math.Sin(_gameOverTimer * 3.0);
        _spriteBatch.DrawString(_hudFont, sub,
            new Vector2(screenW / 2 - subSize.X / 2, screenH / 2 + 30),
            Color.White * alpha);

        // Stats
        string stats = $"Wave reached: {_waveManager.CurrentWave + 1}/10   Lives: {_gameState.Lives}   Gold: {_gameState.Currency}";
        Vector2 statsSize = _hudFont.MeasureString(stats);
        _spriteBatch.DrawString(_hudFont, stats,
            new Vector2(screenW / 2 - statsSize.X / 2, screenH / 2 + 70),
            Color.Gray);

        _spriteBatch.End();
    }

    // --- Drawing helpers ---

    private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, int segments = 32)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle1 = MathHelper.TwoPi * i / segments;
            float angle2 = MathHelper.TwoPi * (i + 1) / segments;
            Vector2 p1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
            Vector2 p2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
            DrawLine(sb, p1, p2, color);
        }
    }

    private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        sb.Draw(_pixelTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 2),
            null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
    }

    private void DrawRectBorder(Rectangle rect, Color color, int thickness)
    {
        // Top
        _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
