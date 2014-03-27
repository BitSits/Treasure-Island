﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Level : IDisposable
    {
        #region Fields

        public int Score { get; private set; }

        public bool IsLevelUp { get; private set; }
        public bool ReloadLevel { get; private set; }
        int levelIndex;

        GameContent gameContent;
        World world = new World(new Vector2(), true);

        Tile[,] tiles;
        Body groundBody;

        Player player;
        Ship ship;

        Camera2D camera;

        List<Vector2> hearts = new List<Vector2>(), collets = new List<Vector2>();

        List<Enemy> enemies = new List<Enemy>();

        Vector2 exit;

        #endregion

        #region Initialization


        public Level(GameContent gameContent, int levelIndex)
        {
            this.gameContent = gameContent;
            this.levelIndex = levelIndex;

            camera = new Camera2D(gameContent.viewportSize);

            groundBody = world.CreateBody(new BodyDef());
            LoadTiles(levelIndex);

            UpdateCameraChaseTarget(new GameTime());

            Vector2[] v = {Vector2.Zero, new Vector2(tiles.GetLength(0), 0),
                              new Vector2(tiles.GetLength(0), tiles.GetLength(1)), 
                              new Vector2(0, tiles.GetLength(1)), Vector2.Zero};

            for (int i = 0; i < v.Length - 1; i++)
            {
                PolygonShape ps = new PolygonShape();
                ps.SetAsEdge(v[i] * Tile.Width / gameContent.b2Scale, v[i + 1] * Tile.Width / gameContent.b2Scale);

                groundBody.CreateFixture(ps, 0);
            }
        }


        private void LoadTiles(int levelIndex)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            lines = gameContent.content.Load<List<string>>("Levels/level" + levelIndex.ToString("0"));

            width = lines[0].Length;

            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < lines.Count; ++y)
            {
                if (lines[y].Length != width)
                    throw new Exception(String.Format(
                        "The length of line {0} is different from all preceeding lines.", lines.Count));

                for (int x = 0; x < lines[0].Length; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    LoadTile(tileType, x, y);
                }
            }
        }


        private void LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                case '1': enemies.Add(new Enemy(world, gameContent, 1,
                    Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height)))); LoadSeaTile(x, y); 
                    break;
                case 'S': ship = new Ship(gameContent.ship,
                    Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height))); LoadSeaTile(x, y); 
                    break;
                case '.': LoadSeaTile(x, y);                     break;

                case '0': enemies.Add(new Enemy(world, gameContent, 0,
                Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height))));
                    LoadLandTile(x, y); break;
                case '-': LoadLandTile(x, y); break;
                case 'P': player = new Player(gameContent, world,
                    Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height)));
                    camera.Position = Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height));
                    LoadLandTile(x, y); break;

                case 'H': hearts.Add(Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height)));
                    LoadLandTile(x, y); break;
                case 'C': collets.Add(Tile.TileCenter(new Vector2(x * Tile.Width, y * Tile.Height)));
                    LoadLandTile(x, y); break;

                case 'X': exit = new Vector2(x, y) * Tile.Width + new Vector2(Tile.Width) / 2;
                    LoadLandTile(x, y); break;

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format(
                        "Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }

            LoadBoundry(x, y);
        }

        void LoadLandTile(int x, int y)
        {
            tiles[x, y] = new Tile(gameContent.land[gameContent.random.Next(gameContent.land.Length)],
                TileType.land);
        }

        void LoadSeaTile(int x, int y)
        {
            tiles[x, y] = new Tile(gameContent.water[gameContent.random.Next(gameContent.water.Length)],
                TileType.sea);
        }

        void LoadBoundry(int x, int y)
        {
            FixtureDef fd = new FixtureDef();
            fd.userData = "string";
            PolygonShape ps = new PolygonShape();

            if (y > 0 && tiles[x, y].tileType != tiles[x, y - 1].tileType)
            {
                ps.SetAsEdge(new Vector2(x * Tile.Width, y * Tile.Height) / gameContent.b2Scale,
                    new Vector2((x + 1) * Tile.Width, y * Tile.Height) / gameContent.b2Scale);

                fd.shape = ps;
                groundBody.CreateFixture(fd);
            }

            if (x > 0 && tiles[x, y].tileType != tiles[x - 1, y].tileType)
            {
                ps.SetAsEdge(new Vector2(x * Tile.Width, y * Tile.Height) / gameContent.b2Scale,
                    new Vector2(x * Tile.Width, (y + 1) * Tile.Height) / gameContent.b2Scale);

                fd.shape = ps;
                groundBody.CreateFixture(fd);
            }
        }


        public void Dispose() { }


        #endregion

        #region Update and HandleInput


        public void Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 5, 8);
            
            if (player.inSea)
            {
                ship.position = player.body.Position * gameContent.b2Scale;
                ship.direction = player.direction;
            }

            UpdateCameraChaseTarget(gameTime);

            for (int i = hearts.Count - 1; i >= 0; i--)
            {
                int s = gameContent.heart.Width / 2;
                if (player.HealthBounds.Intersects(new Rectangle((int)hearts[i].X - s, (int)hearts[i].Y - s,
                    s * 2, s * 2)))
                {
                    hearts.RemoveAt(i); player.health += 20;
                    gameContent.soundBank.GetCue("pick").Play();
                }
            }

            for (int i = collets.Count - 1; i >= 0; i--)
            {
                int s = gameContent.collect.Width / 2;
                if (player.HealthBounds.Intersects(new Rectangle((int)collets[i].X - s, (int)collets[i].Y - s,
                    s * 2, s * 2)))
                {
                    collets.RemoveAt(i); Score += 1;
                    gameContent.soundBank.GetCue("pick").Play();
                }
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime, player.body.Position * gameContent.b2Scale);

                if(player.HealthBounds.Intersects(enemies[i].Bounds))
                    player.health = Math.Max(0, player.health - .3f);
            }

            player.Update(gameTime);

            player.CheckCollision(ship.BoundingRectangle);

            if (player.health == 0) ReloadLevel = true;
            if (player.HealthBounds.Contains(new Point((int)exit.X, (int)exit.Y)))
                IsLevelUp = true;
        }

        void UpdateCameraChaseTarget(GameTime gameTime)
        {
            float stiffness = 1800.0f, damping = 600.0f, mass = 50.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate spring force
            Vector2 stretch = camera.Position - player.body.Position * gameContent.b2Scale;
            Vector2 force = -stiffness * stretch - damping * camera.Velocity;

            // Apply acceleration
            Vector2 acceleration = force / mass;
            camera.Velocity += acceleration * elapsed;

            // Apply velocity
            camera.Position += camera.Velocity * elapsed;

            camera.Position.X = MathHelper.Clamp(camera.Position.X, gameContent.viewportSize.X / 2,
                tiles.GetLength(0) * Tile.Width - gameContent.viewportSize.X / 2);
            camera.Position.Y = MathHelper.Clamp(camera.Position.Y, gameContent.viewportSize.Y / 2,
                tiles.GetLength(1) * Tile.Width - gameContent.viewportSize.Y / 2);
        }


        public void HandleInput(InputState input, int playerIndex)
        {
            player.direction = Vector2.Zero;
            if (input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.W)
                || input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.Up)) player.direction.Y = -1;
            else if (input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.S)
                || input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.Down)) player.direction.Y = 1;
            if (input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.A)
                || input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.Left)) player.direction.X = -1;
            else if (input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.D)
                || input.CurrentKeyboardStates[playerIndex].IsKeyDown(Keys.Right)) player.direction.X = 1;
        }


        #endregion

        #region Draw


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.SaveState, camera.Transform);

            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Vector2 v = new Vector2(x * Tile.Width, y * Tile.Height);
                    if (tiles[x, y].tileType == TileType.sea) spriteBatch.Draw(tiles[x, y].texture, v, Color.White);
                }

            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Vector2 v = new Vector2(x * Tile.Width, y * Tile.Height);
                    if (tiles[x, y].tileType == TileType.land)
                        spriteBatch.Draw(gameContent.sandBed,
                            v + new Vector2(Tile.Width - gameContent.sandBed.Width) / 2, Color.Gainsboro);
                }

            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Vector2 v = new Vector2(x * Tile.Width, y * Tile.Height);
                    if (tiles[x, y].tileType == TileType.land) spriteBatch.Draw(tiles[x, y].texture, v, Color.White);
                }

            spriteBatch.Draw(gameContent.crossMark, exit - new Vector2(gameContent.crossMark.Width) / 2,
                Color.White);

            for (int i = 0; i < hearts.Count; i++)
                spriteBatch.Draw(gameContent.heart, hearts[i] - new Vector2(gameContent.heart.Width) / 2,
                    Color.White);

            for (int i = 0; i < collets.Count; i++)
                spriteBatch.Draw(gameContent.collect, collets[i] - new Vector2(gameContent.collect.Width) / 2,
                    Color.White);

            player.Draw(gameTime, spriteBatch);

            ship.Draw(gameTime, spriteBatch);

            for (int i = 0; i < enemies.Count; i++) enemies[i].Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();

            player.DrawHealthBar(spriteBatch);

            spriteBatch.Draw(gameContent.collect, new Vector2(100, 32), null, Color.White, 0,
                new Vector2(gameContent.collect.Width) / 2, 0.5f, SpriteEffects.None, 1);

            spriteBatch.DrawString(gameContent.gameFont, "x " + Score, new Vector2(120, 25), Color.White, 0,
                Vector2.Zero, 0.8f, SpriteEffects.None, 1);
        }


        #endregion
    }
}
