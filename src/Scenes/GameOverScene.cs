﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ovow.Framework;
using Ovow.Framework.Components;
using Ovow.Framework.Sounds;
using Ovow.Framework.Sprites;

namespace TypingGame.Scenes
{
    internal sealed class GameOverScene : SceneBase
    {
        private const string ExitGameHint = "游戏结束，请按【空格】键退出";
        private BackgroundMusic bgm;
        private SoundEffect bgmEffect;
        private bool disposed = false;
        private SpriteFont font;
        private Texture2D gameOverTexture;

        public GameOverScene(IOvowGame game)
            : base(game)
        { }

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

            gameOverTexture = contentManager.Load<Texture2D>("game_over");
            Add(new DumbSprite(this, gameOverTexture, new Vector2((ViewportWidth - gameOverTexture.Width) / 2, (ViewportHeight - gameOverTexture.Height) / 2 - 150)));

            font = contentManager.Load<SpriteFont>("font");
            var beginGameTextSize = font.MeasureString(ExitGameHint);
            Add(new Text(ExitGameHint, this, font, Color.Yellow,
                new Vector2((ViewportWidth - beginGameTextSize.X) / 2, (ViewportHeight - beginGameTextSize.Y) / 2 - 20)));

            bgmEffect = contentManager.Load<SoundEffect>("ending");
            bgm = new BackgroundMusic(new[] { bgmEffect }, 0.2F, false);
            Add(bgm);
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                End();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    bgm.Stop();
                    if (!bgmEffect.IsDisposed)
                    {
                        bgmEffect.Dispose();
                    }
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}