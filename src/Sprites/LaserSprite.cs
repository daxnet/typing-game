using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ovow.Framework.Scenes;
using Ovow.Framework.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingGame.Sprites
{
    /// <summary>
    /// Represents the laser sprite.
    /// </summary>
    /// <seealso cref="Ovow.Framework.Sprites.Sprite" />
    internal sealed class LaserSprite : Sprite
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LaserSprite"/> class.
        /// </summary>
        /// <param name="scene">The scene to which this laser object belongs.</param>
        /// <param name="texture">The texture of the laser.</param>
        /// <param name="position">The initial position where the laser should appear.</param>
        /// <param name="letter">The letter that the laser targets to.</param>
        public LaserSprite(IScene scene, Texture2D texture, Vector2 position, char letter) : base(scene, texture, position)
        {
            Letter = letter;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the letter that the laser targets to.
        /// </summary>
        /// <value>
        /// The letter.
        /// </value>
        public char Letter { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Updates the game with the given <see cref="GameTime"/>.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            this.Y -= 20;
            base.Update(gameTime);
        }

        #endregion Public Methods
    }
}
