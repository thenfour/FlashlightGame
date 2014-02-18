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

	public class Player : Layer
	{
		public const int moveSpeed = 4;

		public int playerLayer = 1;

		Texture2D playerSprite;
		Texture2D aimSprite;
		Texture2D flashlightBeamMul;

		public Vector2 AimPosition;
		public float AimDirection = 0.0f;

		public MovementHelper movement;

		bool flashlightOn = true;
		KeyDownHelper flashlightToggler = new KeyDownHelper();

		public Player(PitchBlackGame game, Level level, Vector2 startingPosition)
			: base(game, level, null, false)
		{
			movement = new MovementHelper();
			movement.CurrentPosition = startingPosition;
		}

		public override void RefreshMode()
		{
			base.RefreshMode();
			playerSprite = game.Content.Load<Texture2D>("player");
			aimSprite = game.Content.Load<Texture2D>("aim");
			flashlightBeamMul = game.Content.Load<Texture2D>("flashlightbeam-mul");
		}

		public override void Update(KeyboardState keyboardState, MouseState mouseState, LayerMap layerMap)
		{
			base.Update(keyboardState, mouseState, layerMap);

			AimPosition.X = (int)Math.Round((float)mouseState.X + level.scrollingMovement.X);
			AimPosition.Y = (int)Math.Round((float)mouseState.Y + level.scrollingMovement.Y);

			movement.TryMove(new Vector2(
				(keyboardState.IsKeyDown(Keys.Left) ? -moveSpeed : 0) + (keyboardState.IsKeyDown(Keys.Right) ? moveSpeed : 0),
				(keyboardState.IsKeyDown(Keys.Up) ? -moveSpeed : 0) + (keyboardState.IsKeyDown(Keys.Down) ? moveSpeed : 0)),
				layerMap);

			flashlightToggler.Update(keyboardState.IsKeyDown(Keys.F1), delegate() { flashlightOn = !flashlightOn; });

			AimDirection = (float)Math.Atan2(AimPosition.X - movement.CurrentPosition.X, movement.CurrentPosition.Y - AimPosition.Y) - MathHelper.PiOver2;

			playerLayer = layerMap.GetMovementLayer(movement.CurrentPosition);
		}

		public override void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (flashlightOn)
			{
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
				game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
				game.GraphicsDevice.RenderState.SourceBlend = Blend.BlendFactor;
				game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
				game.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
				game.GraphicsDevice.RenderState.BlendFactor = Color.White;

				// rotation, origin, scale
				level.DrawSprite(flashlightBeamMul, movement.CurrentPosition, AimDirection, new Vector2(0, 527), 0.5f);
				spriteBatch.End();
			}
		}

		public override void RenderScene(SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.RenderScene(spriteBatch, gameTime);
		}

		public override void RenderHUD(SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			level.DrawSprite(playerSprite, movement.CurrentPosition, 0.5f);
			level.DrawSprite(aimSprite, AimPosition, 0.5f);
			spriteBatch.End();
		}
	}
}
