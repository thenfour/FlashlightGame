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
	////////////////////////////////////////////////////////////////////////
	namespace AnimationScriptObjects
	{
		public class Image<bool>
		{
			Vector2 origin;
		}

		public class BodyPart
		{
			// regions
		}

		public class BodyGesture
		{
			TimeSpan timeStarted;

			public BodyPart FindPart(string partName, GameTime gameTime, bool flashlightOn, float moveDirection, float aimDirection, float overheadAngle, Vector2 worldPosition)
			{
			}

			public void Draw(GameTime gameTime, bool flashlightOn, float moveDirection, float aimDirection, float overheadAngle, Vector2 worldPosition)
			{
				// frame #
			}
		}

		public class AnimatedCharacter
		{
			BodyGesture[] gestures;
			BodyGesture currentGesture;

			public AnimatedCharacter()
			{
				gestures = new BodyGesture[1];
			}

			public void Draw(GameTime gameTime, bool flashlightOn, float moveDirection, float aimDirection, float overheadAngle, Vector2 worldPosition)
			{
			}
		}

		public enum ArgID
		{
			FlashlightOn,
			LookAngle,
			AimAngle,
			OverheadAngle
		}

		public class Arg : Attribute
		{
			public Arg(ArgID id, int val)
			{
			}
			public Arg(ArgID id, int minInclusive, int maxExclusive)
			{
			}
		}
	}

	namespace SpecificAnimations
	{
		using AnimationScriptObjects;

		[Arg(ArgID.AimAngle, 0), Arg(ArgID.AimAngle, 0)]
		public class Img245 : Image { }
	}


	////////////////////////////////////////////////////////////////////////
	public class PitchBlackGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		KeyDownHelper fullScreenToggler = new KeyDownHelper();
		KeyDownHelper lightingToggler = new KeyDownHelper();
		KeyDownHelper levelSwitcher = new KeyDownHelper();
		bool applyLighting = true;
		RenderTarget2D worldRenderTarget;
		RenderTarget2D lightingRenderTarget;

		// when shader effects are chained,
		// we render from A -> B -> A -> B
		// where each -> is a shader effect applied.
		RenderTarget2D shaderTargetA;
		RenderTarget2D shaderTargetB;

		Texture2D hudSprite;

		SpriteFont debugFont;

		public List<ShaderEffect> ShaderEffects = new List<ShaderEffect>();

		int currentLevel;
		Level[] levels;

		Level CurrentLevel
		{
			get
			{
				return levels[currentLevel];
			}
		}

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

		void RefreshMode()
		{
			debugFont = Content.Load<SpriteFont>("DebugFont");
			CurrentLevel.RefreshMode();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			hudSprite = Content.Load<Texture2D>("hud");

			levels = new Level[]
			{
				new CourtyardLevel(this),
				new RoomLevel(this),
				new StadiumLevel(this),
				new TileLevel(this),
			};
			currentLevel = 0;

			//waveFx = new WaveEffect(Content);
			//zoomBlur = new ZoomBlurEffect(Content);
			//pixelate = new PixelateEffect(Content);
			//ShaderEffects.Add(pixelate);
			//ShaderEffects.Add(zoomBlur);

			ShaderEffects.Add(new ZoomBlurEffect(Content));

			shaderTargetA = Utility.CloneRenderTarget(GraphicsDevice);
			shaderTargetB = Utility.CloneRenderTarget(GraphicsDevice);
			worldRenderTarget = Utility.CloneRenderTarget(GraphicsDevice);
			lightingRenderTarget = Utility.CloneRenderTarget(GraphicsDevice);

			RefreshMode();
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

			CurrentLevel.Update(keyboardState, mouseState, gameTime);

			// update effects
			foreach (ShaderEffect fx in ShaderEffects)
			{
				fx.Update(keyboardState, mouseState, gameTime);
			}

			fullScreenToggler.Update(keyboardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyDown(Keys.LeftAlt), delegate()
			{
				graphics.ToggleFullScreen();
				lightingRenderTarget = Utility.CloneRenderTarget(GraphicsDevice);
				RefreshMode();
			});

			levelSwitcher.Update(keyboardState.IsKeyDown(Keys.F5), delegate()
			{
				currentLevel++;
				if (currentLevel == levels.Length)
					currentLevel = 0;
				RefreshMode();
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
			GraphicsDevice.SetRenderTarget(0, lightingRenderTarget);
			CurrentLevel.RenderLighting(spriteBatch, gameTime);

			// RENDER SCENERY
			GraphicsDevice.SetRenderTarget(0, worldRenderTarget);
			GraphicsDevice.Clear(Color.Purple);
			CurrentLevel.RenderScene(spriteBatch, gameTime);

			// APPLY LIGHTING LAYER
			if (applyLighting)
			{
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
				graphics.GraphicsDevice.RenderState.SourceBlend = Blend.DestinationColor;
				graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.SourceColor;
				spriteBatch.Draw(lightingRenderTarget.GetTexture(), Vector2.Zero, Color.White);
				spriteBatch.End();
			}

			// world is rendered. render pixel shaders to it
			RenderTarget2D lastTarget = worldRenderTarget;
			RenderTarget2D source = worldRenderTarget;
			RenderTarget2D target = shaderTargetA;
			if (ShaderEffects.Count == 0)
			{
				target = worldRenderTarget;
			}
			else
			{
				bool alternate = false;
				foreach (ShaderEffect fx in ShaderEffects)
				{
					GraphicsDevice.SetRenderTarget(0, target);
					fx.Apply(source.GetTexture(), GraphicsDevice, spriteBatch);
					lastTarget = target;
					alternate = !alternate;
					if (alternate)
					{
						source = shaderTargetA;
						target = shaderTargetB;
					}
					else
					{
						source = shaderTargetB;
						target = shaderTargetA;
					}
				}
			}

			GraphicsDevice.SetRenderTarget(0, null);

			// RENDER WORLD + HUD
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
			spriteBatch.Draw(lastTarget.GetTexture(), Vector2.Zero, Color.White);
			spriteBatch.Draw(hudSprite, Vector2.Zero, Color.White);
			spriteBatch.End();

			CurrentLevel.RenderHUD(spriteBatch, gameTime);

			// render debug info
			spriteBatch.Begin();
			spriteBatch.DrawString(debugFont, string.Format("Player {0}\r\nScroll {1}",
				CurrentLevel.player.movement.CurrentPosition.ToString(),
				new Vector2(CurrentLevel.scrollingMovement.X, CurrentLevel.scrollingMovement.Y).ToString()
				), new Vector2(0, 0), new Color(1.0f, 1.0f, 1.0f, 0.7f));
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

