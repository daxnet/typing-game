using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Ovow.Framework;
using Ovow.Framework.Scenes;
using Ovow.Framework.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingGame.Scenes
{
    internal abstract class SceneBase : Scene
    {
        private Texture2D cloudTexture;
        private Texture2D grassTexture;
        private Texture2D sunTexture;
        private Texture2D treeTexture;
        private bool disposed = false;

        protected SceneBase(IOvowGame game)
            : base(game)
        {

        }

        public override void Load(ContentManager contentManager)
        {
            // Loads texture for background elements.
            sunTexture = contentManager.Load<Texture2D>("sun");
            cloudTexture = contentManager.Load<Texture2D>("cloud");
            treeTexture = contentManager.Load<Texture2D>("tree");
            grassTexture = contentManager.Load<Texture2D>("grass");

            var sunSprite = new DumbSprite(this, sunTexture, new Vector2(10, 10));
            Add(sunSprite);

            var cloudSprite = new DumbSprite(this, cloudTexture, new Vector2(ViewportWidth - cloudTexture.Width - 50, 50));
            Add(cloudSprite);

            var grassSprite = new DumbSprite(this, grassTexture, new Vector2(0, ViewportHeight - grassTexture.Height));
            Add(grassSprite);

            var treeSprite = new DumbSprite(this, treeTexture, new Vector2(ViewportWidth - treeTexture.Width - 30, ViewportHeight - treeTexture.Height - 100));
            Add(treeSprite);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    treeTexture.Dispose();
                    cloudTexture.Dispose();
                    sunTexture.Dispose();
                    grassTexture.Dispose();
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
