using System;
using System.Collections.Generic;
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
	////////////////////////////////////////////////////////////////////
	public class Tile
	{
		public Texture2D texture;
	}

	// a layer may be composed of many tiles. they must be equal size!
	public class CompositeTexture
	{
		Tile[,] tiles;
	}

	// a layer may have many tiles used in rendering it - for example 5 separate tiles to draw
	// the entire background.
	public class Layer2
	{
		Vector2 origin = Vector2.Zero;
		float parallaxFactor = 1.0f;
		bool tileVertically = false;
		bool tileHorizontally = false;

		void Draw()
		{
		}
	}


	////////////////////////////////////////////////////////////////////
	public class PitchBlackGame : Microsoft.Xna.Framework.Game
	{
		public static RenderTarget2D CloneRenderTarget(GraphicsDevice device)
		{
			return new RenderTarget2D(device,
					device.PresentationParameters.BackBufferWidth,
					device.PresentationParameters.BackBufferHeight,
					1,
					device.DisplayMode.Format,
					device.PresentationParameters.MultiSampleType,
					device.PresentationParameters.MultiSampleQuality
			);
		}

		GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		KeyDownHelper fullScreenToggler = new KeyDownHelper();
		KeyDownHelper lightingToggler = new KeyDownHelper();
		bool applyLighting = true;
		RenderTarget2D tempRenderTarget;

		Level level;

		public PitchBlackGame()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 600;
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//level = new RoomLevel(this);
			level = new StadiumLevel(this);

			tempRenderTarget = CloneRenderTarget(GraphicsDevice);
			level.RefreshMode();

			TestLayerMap tlm = new TestLayerMap();
			MovementHelper m = new MovementHelper();
			for (int i = 0; i < tlm.TestCount; ++i)
			{
				int startingX;
				int startingY;
				int destX;
				int destY;
				int expectedX;
				int expectedY;
				tlm.GetTestInfo(i, out startingX, out startingY, out destX, out destY, out expectedX, out expectedY);
				m.CurrentPosition.X = startingX;
				m.CurrentPosition.Y = startingY;
				m.TryMove(new Vector2(destX - startingX, destY - startingY), tlm);
				System.Diagnostics.Debug.WriteLine(string.Format("Movement test {0}, Actual/Expected ({1},{2})/({3},{4}): {5}",
					i,
					m.CurrentPosition.X,
					m.CurrentPosition.Y,
					expectedX,
					expectedY,
					m.CurrentPosition == new Vector2(expectedX, expectedY) ? "pass" : "FAIL"
					));
			}
		}

		protected override void UnloadContent()
		{
		}

		bool GetScreenBounds(out int Width, out int Height)
		{
			foreach (var Screen in System.Windows.Forms.Screen.AllScreens)
			{
				if (String.Compare(Screen.DeviceName, this.Window.ScreenDeviceName, true) == 0)
				{
					Width = Screen.Bounds.Width;
					Height = Screen.Bounds.Height;
					return true;
				}
			}

			var Size = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
			Width = Size.Width;
			Height = Size.Height;
			return false;
		}

		protected override void Update(GameTime gameTime)
		{
			KeyboardState keyboardState = Keyboard.GetState();
			MouseState mouseState = Mouse.GetState();
			level.Update(keyboardState, mouseState, gameTime);
			fullScreenToggler.Update(keyboardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyDown(Keys.LeftAlt), delegate()
			{
				graphics.ToggleFullScreen();
				tempRenderTarget = CloneRenderTarget(GraphicsDevice);
				level.RefreshMode();
			});

			lightingToggler.Update(keyboardState.IsKeyDown(Keys.F4), delegate()
			{
				applyLighting = !applyLighting;
			});

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// RENDER LIGHTING LAYER
			GraphicsDevice.SetRenderTarget(0, tempRenderTarget);
			level.RenderLighting(spriteBatch, gameTime);

			// RENDER SCENERY
			GraphicsDevice.SetRenderTarget(0, null);
			GraphicsDevice.Clear(Color.Purple);
			level.RenderScene(spriteBatch, gameTime);

			// APPLY LIGHTING LAYER
			if (applyLighting)
			{
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
				graphics.GraphicsDevice.RenderState.SourceBlend = Blend.DestinationColor;
				graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.SourceColor;
				spriteBatch.Draw(tempRenderTarget.GetTexture(), new Vector2(0, 0), Color.White);
				spriteBatch.End();
			}

			// RENDER HUD
			level.RenderHUD(spriteBatch, gameTime);

			base.Draw(gameTime);
		}
	}
}

