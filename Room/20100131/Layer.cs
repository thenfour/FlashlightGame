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
	public class Layer
	{
		bool isBackground = false;
		public const int blackLevel = 2;
		protected PitchBlackGame game;
		public Texture2D scenery = null;
		Texture2D sceneryBlack = null;
		string sceneryName = null;
		protected Level level;

		public Layer(PitchBlackGame game, Level level, string sceneryName, bool isBackground)
		{
			this.level = level;
			this.isBackground = isBackground;
			this.game = game;
			this.sceneryName = sceneryName;
		}

		public virtual void RefreshMode()
		{
			if (!string.IsNullOrEmpty(sceneryName))
			{
				scenery = game.Content.Load<Texture2D>(sceneryName);
				sceneryBlack = new Texture2D(scenery.GraphicsDevice, scenery.Width, scenery.Height, scenery.LevelCount, scenery.TextureUsage, scenery.Format);
				{
					Color[] cs = new Color[scenery.Width * scenery.Height];
					scenery.GetData(cs);
					for (int i = 0; i < cs.Length; ++i)
					{
						cs[i].R = blackLevel;
						cs[i].G = blackLevel;
						cs[i].B = blackLevel;
					}
					sceneryBlack.SetData(cs);
				}
			}
		}

		public virtual void Update(KeyboardState keyboardState, MouseState mouseState, LayerMap layerMap)
		{
		}

		// need to draw additive lighting to be multiplied later.
		public virtual void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (scenery != null)
			{
				// start by drawing "black" to the alpha mask of the scenery for this layer, basically to reset this area to what it was before
				spriteBatch.Begin();
				level.DrawSprite(sceneryBlack, Vector2.Zero);
				spriteBatch.End();
			}
		}

		public virtual void RenderScene(SpriteBatch spriteBatch, GameTime gameTime)
		{
			// draw scenery
			if (scenery != null)
			{
				spriteBatch.Begin();
				level.DrawSprite(scenery, Vector2.Zero);
				spriteBatch.End();
			}
		}

		public virtual void RenderHUD(SpriteBatch spriteBatch, GameTime gameTime)
		{
		}
	}
}
