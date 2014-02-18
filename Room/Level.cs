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
	public abstract class Level
	{
		protected PitchBlackGame game;
		protected List<Layer> layers = new List<Layer>();
		public Player player;

		public LayerMap LayerMap;

		private Vector2 startingPosition;
		public ScrollingMovement2D scrollingMovement;

		// required for scrolling behavior to know its boundaries
		protected abstract Vector2 UntiledLevelSize { get; }
		protected virtual bool InfiniteSizeX { get { return false; } }
		protected virtual bool InfiniteSizeY { get { return false; } }


		public Level(PitchBlackGame game, CompositeTexture layerMapTexture, Vector2 startingPosition)
		{
			this.game = game;
			player = new Player(game, this, startingPosition);
			layers.Add(player);
			LayerMap = new LayerMap(game, layerMapTexture);
			this.startingPosition = startingPosition;
		}

	
		public void DrawSprite(Texture2D texture, Vector2 destPosition_WorldCoords)
		{
			DrawSprite(texture, destPosition_WorldCoords, Color.White);
		}

		public void DrawSprite(Texture2D texture, Vector2 destPosition_WorldCoords, float alpha)
		{
			DrawSprite(texture, destPosition_WorldCoords, new Color(1.0f, 1.0f, 1.0f, alpha));
		}

		void CalculateSpriteRenderCoords(Texture2D texture, Vector2 destPosition_WorldCoords, out Vector2 destPos, out Rectangle sourceRect)
		{
			destPos = new Vector2(destPosition_WorldCoords.X - scrollingMovement.X, destPosition_WorldCoords.Y - scrollingMovement.Y);

			sourceRect = new Rectangle(0, 0,
				(int)Math.Round(Math.Min(texture.Width, game.GraphicsDevice.Viewport.Width - destPos.X)),
				(int)Math.Round(Math.Min(texture.Height, game.GraphicsDevice.Viewport.Height - destPos.Y))
				);

			if (destPos.X < 0)
			{
				sourceRect.Width += (int)Math.Round(destPos.X);
				sourceRect.X = -(int)Math.Round(destPos.X);
				destPos.X = 0;
			}

			if (destPos.Y < 0)
			{
				sourceRect.Height += (int)Math.Round(destPos.Y);
				sourceRect.Y = -(int)Math.Round(destPos.Y);
				destPos.Y = 0;
			}
		}

		public void DrawSprite(Texture2D texture, Vector2 destPosition_WorldCoords, float rotation, Vector2 origin, float scale)
		{
			Vector2 destPos;
			destPos = new Vector2(destPosition_WorldCoords.X - scrollingMovement.X, destPosition_WorldCoords.Y - scrollingMovement.Y);
			game.spriteBatch.Draw(texture, destPos, null, Color.White, rotation, origin, scale, SpriteEffects.None, 0);
		}

		public void DrawSprite(Texture2D texture, Vector2 destPosition_WorldCoords, Color tint)
		{
			Vector2 destPos;
			Rectangle sourceRect;
			CalculateSpriteRenderCoords(texture, destPosition_WorldCoords, out destPos, out sourceRect);
			if (sourceRect.Width <= 0)
				return;
			if (sourceRect.Height <= 0)
				return;
			game.spriteBatch.Draw(texture, destPos, sourceRect, tint);
		}

		public void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
		{
			foreach (Layer l in layers)
			{
				l.RenderLighting(spriteBatch, gameTime);
			}
		}

		public void RenderScene(SpriteBatch spriteBatch, GameTime gameTime)
		{
			foreach (Layer l in layers)
			{
				l.RenderScene(spriteBatch, gameTime);
			}
		}

		public void RenderHUD(SpriteBatch spriteBatch, GameTime gameTime)
		{
			foreach (Layer l in layers)
			{
				l.RenderHUD(spriteBatch, gameTime);
			}
		}

		public void Update(KeyboardState keyboardState, MouseState mouseState, GameTime gameTime)
		{
			foreach (Layer l in layers)
			{
				l.Update(keyboardState, mouseState);
			}

			// rearrange player in layers
			layers.Remove(player);
			layers.Insert(player.playerLayer, player);

			scrollingMovement.Update(player.movement.CurrentPosition, player.AimPosition, game.GraphicsDevice.Viewport, UntiledLevelSize, InfiniteSizeX, InfiniteSizeY, gameTime);
		}

		public void RefreshMode()
		{
			if (scrollingMovement == null)
			{
				scrollingMovement = new ScrollingMovement2D(startingPosition, game.GraphicsDevice.Viewport, UntiledLevelSize, InfiniteSizeX, InfiniteSizeY);
			}
			LayerMap.RefreshMode();
			foreach (Layer l in layers)
			{
				l.RefreshMode();
			}
		}
	}
}
