using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace PitchBlack
{
	// a layer can actually be composed of many textures, to create enormous layers
	// layers can also be rendered as wrapping in X or Y direction, or parallaxed.
	public class Layer
	{
		protected Level level;
		protected PitchBlackGame game;
		protected CompositeTexture texture = null;

		public override string ToString()
		{
			if (texture == null)
				return "(unnamed layer)";
			return texture.ToString();
		}

		public virtual Vector2 UntiledLayerSize { get { return texture.UntiledSize; } }
		public virtual bool InfiniteSizeX { get { return false; } }
		public virtual bool InfiniteSizeY { get { return false; } }

		// if you have 6 tiles, 2 columns of 3, then pass 2 into tileColumns, and 6 strings into tileTextureNames.
		public Layer(PitchBlackGame game, Level level, CompositeTexture texture)
		{
			this.texture = texture;
			this.level = level;
			this.game = game;
		}

		public virtual void RefreshMode()
		{
			if (texture != null)
				texture.RefreshMode();
		}

		public virtual void Update(KeyboardState keyboardState, MouseState mouseState)
		{
		}

		// need to draw additive lighting to be multiplied later.
		public virtual void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (texture != null)
			{
				spriteBatch.Begin();
				texture.DrawLighting(Vector2.Zero, level.scrollingMovement, spriteBatch);
				spriteBatch.End();
			}
		}

		public virtual void RenderScene(SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (texture != null)
			{
				spriteBatch.Begin();
				texture.DrawScenery(Vector2.Zero, level.scrollingMovement, spriteBatch);
				spriteBatch.End();
			}
		}

		public virtual void RenderHUD(SpriteBatch spriteBatch, GameTime gameTime)
		{
		}
	}
}
