using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Ovow.Framework;
using Ovow.Framework.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingGame.Scenes
{
    internal sealed class EndScene : Scene
    {
        public EndScene(IOvowGame game)
            : base(game)
        { }

        public override void Load(ContentManager contentManager)
        {
            // throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                End();
            }
        }
    }
}
