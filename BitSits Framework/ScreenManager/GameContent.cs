﻿using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// All the Contents of the Game is loaded and stored here
    /// so that all other screen can copy from here
    /// </summary>
    public class GameContent
    {
        public ContentManager content;
        public Vector2 viewportSize;
        
        public Random random = new Random();

        public float b2Scale = 30;

        // Textures
        public Texture2D blank, gradient;
        public Texture2D menuBackground;

        public Texture2D playerIdle, playerWalk, playerDie, ship;

        public Texture2D[] enemy = new Texture2D[2];

        public Texture2D healthBar, collect, heart, crossMark;

        public Texture2D[] land = new Texture2D[4], water = new Texture2D[2];
        public Texture2D sandBed;

        public Texture2D gameOver, retry, levelUp, tutorial;

        // Fonts
        //public SpriteFont debugFont;
        public SpriteFont gameFont;

        // Audio objects
        public AudioEngine audioEngine;
        public SoundBank soundBank;
        public WaveBank waveBank;
        

        /// <summary>
        /// Load GameContents
        /// </summary>
        public GameContent(GameComponent screenManager)
        {
            content = screenManager.Game.Content;
            Viewport viewport = screenManager.Game.GraphicsDevice.Viewport;
            viewportSize = new Vector2(viewport.Width, viewport.Height);

            blank = content.Load<Texture2D>("Graphics/blank");
            gradient = content.Load<Texture2D>("Graphics/gradient");
            menuBackground = content.Load<Texture2D>("Graphics/menuBackground");

            playerIdle = content.Load<Texture2D>("Graphics/playerIdle");
            playerWalk = content.Load<Texture2D>("Graphics/playerWalk");
            playerDie = content.Load<Texture2D>("Graphics/playerDie");

            ship = content.Load<Texture2D>("Graphics/ship");

            for (int i = 0; i < land.Length; i++) land[i] = content.Load<Texture2D>("Graphics/land" + i);

            for (int i = 0; i < water.Length; i++) water[i] = content.Load<Texture2D>("Graphics/water" + i);

            sandBed = content.Load<Texture2D>("Graphics/sandBed");

            for (int i = 0; i < enemy.Length; i++) enemy[i] = content.Load<Texture2D>("Graphics/enemy" + i);

            healthBar = content.Load<Texture2D>("Graphics/healthBar");

            heart = content.Load<Texture2D>("Graphics/heart");
            collect = content.Load<Texture2D>("Graphics/collect");

            crossMark = content.Load<Texture2D>("Graphics/crossMark");

            gameOver = content.Load<Texture2D>("Graphics/gameOver");
            retry = content.Load<Texture2D>("Graphics/retry");
            levelUp = content.Load<Texture2D>("Graphics/levelUp");
            tutorial = content.Load<Texture2D>("Graphics/tutorial");

            //debugFont = content.Load<SpriteFont>("Fonts/debugFont");
            gameFont = content.Load<SpriteFont>("Fonts/Visitor TT2 BRK 40");
            gameFont.Spacing = 2;
            
            // Initialize audio objects.
            audioEngine = new AudioEngine("Content/Audio/Audio.xgs");
            soundBank = new SoundBank(audioEngine, "Content/Audio/Sound Bank.xsb");
            waveBank = new WaveBank(audioEngine, "Content/Audio/Wave Bank.xwb");

            soundBank.GetCue("music").Play();


            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            screenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload GameContents
        /// </summary>
        public void UnloadContent() { content.Unload(); }
    }
}
