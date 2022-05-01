using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ovow.Framework;
using Ovow.Framework.Components;
using Ovow.Framework.Messaging;
using Ovow.Framework.Scenes;
using Ovow.Framework.Services;
using Ovow.Framework.Sounds;
using Ovow.Framework.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypingGame.Sprites;

namespace TypingGame.Scenes
{
    internal sealed class MainScene : SceneBase
    {
        #region Private Fields

        private const string CurrentLevelTextPattern = @"当前级别：{0} 级";
        private const string CurrentScoreTextPattern = @"当前得分：{0} 分";
        private const string DiagTextPattern = @"【调试：总对象数量】 {0}";
        private const int LetterGenerationInterval = 1500;
        private const string LifeText = @"生命值：";
        private const int ScorePerLevel = 25;
        private const float TotalLifes = 10;

        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
        private readonly List<SoundEffect> bgmMusicEffects = new List<SoundEffect>();
        private readonly Dictionary<char, Texture2D> lettersTextureDict = new Dictionary<char, Texture2D>();
        private readonly List<(float, float)> levelSpeeds = new List<(float, float)>();

        private BackgroundMusic bgm;
        private Text diagText;
        private SpriteFont diagTextFont;
        private bool disposed;
        private Sound explodeSound;
        private SoundEffect explodeSoundEffect;
        private AnimatedSpriteDefinition explosionDefinition;
        private Texture2D explosionTexture;
        private Texture2D laserTexture;
        private TimeSpan letterGenerationTimeSpan = TimeSpan.Zero;
        private TimeSpan letterGenerationTimeSpanThreshold = TimeSpan.FromMilliseconds(LetterGenerationInterval);
        private int level = 0;
        private Text levelText;
        private float life;
        private ProgressBar lifeProgressBar;
        private Text lifeText;
        private int score = 0;
        private SpriteFont scoreLevelTextFont;
        private Text scoreText;
        private bool showDiagText;

        #endregion Private Fields

        #region Public Constructors

        public MainScene(IOvowGame game) : base(game)
        {
            AutoRemoveInactiveComponents = true;

            for (var i = 0; i < 20; i++)
            {
                levelSpeeds.Add(((i + 1) * 0.8F, (i + 2) * 0.8F));
            }

            Subscribe<ReachBoundaryMessage>(HandleReachBoundaryMessage);
            Subscribe<CollisionDetectedMessage>(HandleCollisionDetectedMessage);
            Subscribe<AnimationCompletedMessage>(HandleAnimationCompletedMessage);
        }

        #endregion Public Constructors

        #region Private Properties

        private IEnumerable<LaserSprite> LaserSprites => from p in this where p is LaserSprite select p as LaserSprite;

        private IEnumerable<LetterSprite> LetterSprites => from p in this where p is LetterSprite select p as LetterSprite;

        #endregion Private Properties

        #region Public Methods

        public override void Enter()
        {
            bgm.Play();
        }

        public override void Leave()
        {
            bgm.Stop();
        }

        public override void Load(ContentManager contentManager)
        {
            base.Load(contentManager);

            life = TotalLifes;

            using (var fs = new FileStream("explosion.xml", FileMode.Open, FileAccess.Read))
            {
                explosionDefinition = AnimatedSpriteDefinition.Load(fs);
            }

            // Loads texture for letters.
            for (char i = 'A'; i <= 'Z'; i++)
            {
                lettersTextureDict.Add(i, contentManager.Load<Texture2D>($"{(i - 'A' + 1)}"));
            }

            // Loads texture for laser.
            laserTexture = contentManager.Load<Texture2D>("Laser");

            // Loads texture for explosion.
            explosionTexture = contentManager.Load<Texture2D>("explosion2");

            // Loads font textures.
            diagTextFont = contentManager.Load<SpriteFont>("diagText");
            diagText = new Text(string.Format(DiagTextPattern, this.Count), this, diagTextFont, Color.LightBlue) { CollisionDetective = false };
            this.Add(diagText);

            scoreLevelTextFont = contentManager.Load<SpriteFont>("score");
            scoreText = new Text(string.Format(CurrentScoreTextPattern, this.score),
                this,
                scoreLevelTextFont,
                Color.White,
                new Vector2(ViewportWidth - 200, 3));
            this.Add(scoreText);

            levelText = new Text(string.Format(CurrentLevelTextPattern, this.level + 1),
                this,
                scoreLevelTextFont,
                Color.White,
                new Vector2(ViewportWidth - 200, scoreText.Y + scoreText.TextHeight));
            this.Add(levelText);

            lifeProgressBar = new ProgressBar(this, (ViewportWidth - 210) / 2, 7, 210, 25, ProgressBar.Orientation.HorizontalLeftToRight)
            {
                Minimum = 0,
                Maximum = TotalLifes,
                FillColor = Color.OrangeRed,
                BackgroundColor = Color.CornflowerBlue
            };

            lifeProgressBar.Value = life;
            Add(lifeProgressBar);

            lifeText = new Text(LifeText, this, scoreLevelTextFont, Color.White, new Vector2(lifeProgressBar.Position.X - 80, 5));
            Add(lifeText);

            // Loads the Hit Sound.
            explodeSoundEffect = contentManager.Load<SoundEffect>("explode");
            explodeSound = new Sound(explodeSoundEffect, 0.5F);
            this.Add(explodeSound);

            // Loads the BGM.
            for (var idx = 1; idx <= 2; idx++)
            {
                bgmMusicEffects.Add(contentManager.Load<SoundEffect>($"bgm{idx}"));
            }

            bgm = new BackgroundMusic(bgmMusicEffects, 0.2F);

            Add(bgm);

            // Add game services.
            this.Add(new CollisionDetectionService(this));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                showDiagText = !showDiagText;
            }

            this.letterGenerationTimeSpan += gameTime.ElapsedGameTime;
            if (this.letterGenerationTimeSpan >= letterGenerationTimeSpanThreshold &&
                LetterSprites.Count() < 26)
            {
                var letterIndex = rnd.Next(0, 26);
                var letter = (char)('A' + letterIndex);

                // Make sure that there is no same letter in the current screen.
                while (true)
                {
                    if (LetterSprites.Any(l => l.Letter == letter))
                    {
                        letterIndex = rnd.Next(0, 26);
                        letter = (char)('A' + letterIndex);
                        continue;
                    }

                    break;
                }

                var letterTexture = lettersTextureDict[letter];
                var initialXPos = rnd.Next(2, ViewportWidth - letterTexture.Width);
                var (levelMinSpeed, levelMaxSpeed) = levelSpeeds[level];
                var speed = rnd.NextDouble() * (levelMaxSpeed - levelMinSpeed) + levelMinSpeed;

                var letterSprite = new LetterSprite(this,
                    letterTexture,
                    new Vector2(initialXPos, 0F),
                    letter, (float)speed);

                Add(letterSprite);
                this.letterGenerationTimeSpan = TimeSpan.Zero;
            }

            // Checks key press
            var pressedKeys = Keyboard.GetState().GetPressedKeys();
            LetterSprite hitLetter = null;

            foreach (var ls in LetterSprites)
            {
                if (pressedKeys.Any(pk => (int)pk == ls.Letter))
                {
                    hitLetter = ls;
                    break;
                }
            }

            if (hitLetter != null &&
                !LaserSprites.Any(l => l.Letter == hitLetter.Letter))
            {
                var laserInitialX = hitLetter.X + 18;
                var laserInitialY = ViewportHeight - laserTexture.Height;
                var laserSprite = new LaserSprite(this,
                    laserTexture,
                    new Vector2(laserInitialX, laserInitialY),
                    hitLetter.Letter);
                Add(laserSprite);
            }

            // Updates the diagnostic information text.
            diagText.Visible = showDiagText;
            diagText.Value = string.Format(DiagTextPattern, this.Count);

        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var kvp in lettersTextureDict)
                    {
                        kvp.Value.Dispose();
                    }

                    laserTexture.Dispose();
                    diagTextFont.Texture.Dispose();
                    scoreLevelTextFont.Texture.Dispose();
                    explodeSound.Stop();
                    explodeSoundEffect.Dispose();
                    bgm.Stop();
                    foreach (var musicEffect in bgmMusicEffects)
                    {
                        if (!musicEffect.IsDisposed)
                        {
                            musicEffect.Dispose();
                        }
                    }
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion Protected Methods

        #region Private Methods

        private void HandleAnimationCompletedMessage(object sender, AnimationCompletedMessage message)
        {
            message.Sprite.Stop();
            message.Sprite.IsActive = false;
        }

        private void HandleCollisionDetectedMessage(object sender, CollisionDetectedMessage message)
        {
            if (message.A is LetterSprite letterSprite &&
                    message.B is LaserSprite laserSprite &&
                    letterSprite.Letter == laserSprite.Letter &&
                    letterSprite.IsActive && laserSprite.IsActive)
            {
                var letterSpriteX = letterSprite.X;
                var letterSpriteY = letterSprite.Y;
                explodeSound.Play();
                score++;
                level = score / ScorePerLevel;
                if (level > 19)
                {
                    level = 19;
                }

                if (life < TotalLifes)
                {
                    life += 0.2f;
                }

                lifeProgressBar.Value = life;

                scoreText.Value = string.Format(CurrentScoreTextPattern, score);
                levelText.Value = string.Format(CurrentLevelTextPattern, level + 1);

                letterSprite.IsActive = false;
                laserSprite.IsActive = false;

                var explosionSprite = new AnimatedSprite(this,
                    explosionTexture,
                    new Vector2(letterSpriteX, letterSpriteY),
                    explosionDefinition,
                    "explosion", 400)
                {
                    CollisionDetective = false
                };

                Add(explosionSprite);
            }
        }

        private void HandleReachBoundaryMessage(object sender, ReachBoundaryMessage message)
        {
            if (sender is LetterSprite letterSprite &&
                    message.ReachedBoundary == Boundary.Bottom)
            {
                letterSprite.IsActive = false;
                life--;
                lifeProgressBar.Value = life;
                if (life <= 0)
                {
                    EndTo("gameOver");
                }
            }

            if (sender is LaserSprite laserSprite &&
                message.ReachedBoundary == Boundary.Top)
            {
                laserSprite.IsActive = false;
            }
        }

        #endregion Private Methods
    }
}