using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace monogame_funny_game
{
    public class Map
    {
        public const int TileSize = 64;
        public const int Cols = 20;
        public const int Rows = 10;

        public int[,] Grid;
        public List<Vector2> Waypoints;
        public bool[,] Occupied;

        public Map()
        {
            Grid = new int[Cols, Rows];
            Occupied = new bool[Cols, Rows];
            Waypoints = new List<Vector2>();
            Generate();
        }

        public bool IsBuildable(int col, int row)
        {
            if (col < 0 || col >= Cols || row < 0 || row >= Rows)
                return false;
            return Grid[col, row] == 0 && !Occupied[col, row];
        }

        public void Generate()
        {
            var rand = new Random();
            bool success = false;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                // Reset grid
                Grid = new int[Cols, Rows];

                // Step 1: Pick random spawn row on left edge, exit row on right edge
                int startRow = rand.Next(Rows);
                int endRow = rand.Next(Rows);

                Grid[0, startRow] = 2;           // spawn
                Grid[Cols - 1, endRow] = 3;       // exit

                // Step 2-3: Generate 4-6 intermediate waypoints spread across columns
                int numIntermediates = rand.Next(4, 7);
                var keyPoints = new List<(int col, int row)>();
                keyPoints.Add((0, startRow));

                for (int i = 0; i < numIntermediates; i++)
                {
                    // Divide column range into equal segments
                    int col = (i + 1) * (Cols - 1) / (numIntermediates + 1);
                    col = Math.Clamp(col, 1, Cols - 2);
                    int row = rand.Next(Rows);
                    keyPoints.Add((col, row));
                }

                keyPoints.Add((Cols - 1, endRow));

                // Step 4: Connect waypoints with horizontal-then-vertical L-shaped segments
                for (int i = 0; i < keyPoints.Count - 1; i++)
                {
                    var from = keyPoints[i];
                    var to = keyPoints[i + 1];

                    // Horizontal segment: from (from.col, from.row) to (to.col, from.row)
                    int cMin = Math.Min(from.col, to.col);
                    int cMax = Math.Max(from.col, to.col);
                    for (int c = cMin; c <= cMax; c++)
                    {
                        if (Grid[c, from.row] == 0)
                            Grid[c, from.row] = 1;
                    }

                    // Vertical segment: from (to.col, from.row) to (to.col, to.row)
                    int rMin = Math.Min(from.row, to.row);
                    int rMax = Math.Max(from.row, to.row);
                    for (int r = rMin; r <= rMax; r++)
                    {
                        if (Grid[to.col, r] == 0)
                            Grid[to.col, r] = 1;
                    }
                }

                // Step 5: Validate — at least 60% of tiles remain as grass
                int grassCount = 0;
                int totalTiles = Cols * Rows;
                for (int c = 0; c < Cols; c++)
                    for (int r = 0; r < Rows; r++)
                        if (Grid[c, r] == 0)
                            grassCount++;

                if (grassCount < totalTiles * 0.6)
                    continue;

                // Validate — path is walkable from spawn to exit
                if (BuildWaypoints())
                {
                    success = true;
                    break;
                }
            }

            // Step 7: If all retries fail, use hardcoded snake fallback
            if (!success)
            {
                ApplyFallbackLayout();
                BuildWaypoints();
            }

            // Reset Occupied on every generate
            Occupied = new bool[Cols, Rows];
        }

        /// <summary>
        /// Apply the hardcoded snake fallback layout.
        /// </summary>
        private void ApplyFallbackLayout()
        {
            Grid = new int[Cols, Rows];

            // Layout in [row, col] order for readability; transposed into Grid[col, row]
            int[,] layout = new int[Rows, Cols]
            {
                { 0, 0, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0 },
            };

            for (int c = 0; c < Cols; c++)
                for (int r = 0; r < Rows; r++)
                    Grid[c, r] = layout[r, c];
        }

        /// <summary>
        /// Walk the path from spawn to exit tile-by-tile, then extract direction-change
        /// waypoints (pixel centers). Returns false if the walk cannot reach exit.
        /// </summary>
        private bool BuildWaypoints()
        {
            Waypoints.Clear();

            // Locate spawn and exit tiles
            int spawnCol = -1, spawnRow = -1;
            int exitCol = -1, exitRow = -1;

            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    if (Grid[c, r] == 2) { spawnCol = c; spawnRow = r; }
                    if (Grid[c, r] == 3) { exitCol = c; exitRow = r; }
                }
            }

            if (spawnCol < 0 || exitCol < 0)
                return false;

            // Walk the corridor from spawn to exit
            var path = new List<(int col, int row)>();
            var visited = new HashSet<(int col, int row)>();

            int curCol = spawnCol, curRow = spawnRow;
            path.Add((curCol, curRow));
            visited.Add((curCol, curRow));

            // Direction offsets: right, left, down, up
            int[] dc = { 1, -1, 0, 0 };
            int[] dr = { 0, 0, 1, -1 };

            bool reachedExit = false;

            while (!reachedExit)
            {
                bool found = false;
                for (int d = 0; d < 4; d++)
                {
                    int nc = curCol + dc[d];
                    int nr = curRow + dr[d];

                    if (nc < 0 || nc >= Cols || nr < 0 || nr >= Rows) continue;
                    if (visited.Contains((nc, nr))) continue;

                    int val = Grid[nc, nr];
                    if (val == 1 || val == 3)
                    {
                        curCol = nc;
                        curRow = nr;
                        path.Add((nc, nr));
                        visited.Add((nc, nr));
                        found = true;
                        if (val == 3) reachedExit = true;
                        break;
                    }
                }

                if (!found) break;
            }

            if (!reachedExit || path.Count < 2)
                return false;

            // Extract waypoints at direction changes (corners) + start and end
            Waypoints.Add(ToPixelCenter(path[0].col, path[0].row));

            for (int i = 1; i < path.Count - 1; i++)
            {
                int prevDc = path[i].col - path[i - 1].col;
                int prevDr = path[i].row - path[i - 1].row;
                int nextDc = path[i + 1].col - path[i].col;
                int nextDr = path[i + 1].row - path[i].row;

                if (prevDc != nextDc || prevDr != nextDr)
                {
                    Waypoints.Add(ToPixelCenter(path[i].col, path[i].row));
                }
            }

            Waypoints.Add(ToPixelCenter(path[path.Count - 1].col, path[path.Count - 1].row));
            return true;
        }

        private Vector2 ToPixelCenter(int col, int row)
        {
            return new Vector2(col * TileSize + TileSize / 2, row * TileSize + TileSize / 2);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D grassTexture, Texture2D pathTexture)
        {
            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    var dest = new Rectangle(c * TileSize, r * TileSize, TileSize, TileSize);
                    Texture2D texture = Grid[c, r] == 0 ? grassTexture : pathTexture;
                    spriteBatch.Draw(texture, dest, Color.White);
                }
            }
        }
    }
}
