using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ovow.Framework;
using Ovow.Framework.Components;
using Ovow.Framework.Scenes;
using Ovow.Framework.Sprites;
using Ovow.Framework.Transitions;
using System;

namespace TypingGame.Scenes
{
    internal sealed class PrefaceScene : SceneBase
    {
        private const string BeginGameHint = "请按【空格】键开始游戏";
        private const string CopyrightHint = "作者：凉城三小四（二）班 陈知涵";
        private Texture2D logoTexture;
        private SpriteFont font;
        private bool disposed;

        public PrefaceScene(IOvowGame game)
            : base(game)
        {
            Out = new DelayTransition(this, TimeSpan.FromMilliseconds(100));
            NextSceneName = "main";
        }

        public override void Load(ContentManager contentManager)
        {
            base.Load(contentManager);

            logoTexture = contentManager.Load<Texture2D>("logo");
            Add(new DumbSprite(this, logoTexture, new Vector2((ViewportWidth - logoTexture.Width) / 2, (ViewportHeight - logoTexture.Height) / 2 - 150)));
            font = contentManager.Load<SpriteFont>("font");
            var beginGameTextSize = font.MeasureString(BeginGameHint);
            Add(new Text(BeginGameHint, this, font, Color.Yellow,
                new Vector2((ViewportWidth - beginGameTextSize.X) / 2, (ViewportHeight - beginGameTextSize.Y) / 2)));
            var copyrightTextSize = font.MeasureString(CopyrightHint);
            Add(new Text(CopyrightHint, this, font, Color.White,
                new Vector2((ViewportWidth - copyrightTextSize.X) / 2, ViewportHeight - copyrightTextSize.Y - 15)));
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    logoTexture.Dispose();
                    font.Texture.Dispose();
                }

                disposed = true;
            }
            base.Dispose(disposing);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                End();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }
        }
    }
}