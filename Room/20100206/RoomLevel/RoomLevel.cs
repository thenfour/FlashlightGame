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
	////////////////////////////////////////////////////////////////////
	public class RoomLevel_Layer3 : Layer
	{
		bool candleLit = true;
		Texture2D candle;
		FlickerValues flicker = new FlickerValues();
		KeyDownHelper candleToggler = new KeyDownHelper();

		public RoomLevel_Layer3(PitchBlackGame game, Level level)
			: base(game, level, CompositeTexture.NewSceneryTexture(game.Content, "layer3", false, false))
		{
		}

		public override void Update(KeyboardState keyboardState, MouseState mouseState)
		{
			base.Update(keyboardState, mouseState);
			candleToggler.Update(keyboardState.IsKeyDown(Keys.F2), delegate() { candleLit = !candleLit; });
		}

		public override void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.RenderLighting(spriteBatch, gameTime);

			// render candle
			if (candleLit)
			{
				float fa = flicker.GetNext(gameTime);
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
				game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
				game.GraphicsDevice.RenderState.SourceBlend = Blend.BlendFactor;
				game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
				game.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
				game.GraphicsDevice.RenderState.BlendFactor = new Color(fa, fa, fa, fa);
				level.DrawSprite(candle, Vector2.Zero);
				spriteBatch.End();
			}
		}
		public override void RefreshMode()
		{
			base.RefreshMode();
			candle = game.Content.Load<Texture2D>("candle");
		}
	}

	////////////////////////////////////////////////////////////////////
	public class RoomLevel : Level
	{
		Layer bg;

		public RoomLevel(PitchBlackGame game) :
			base(game, CompositeTexture.NewLayerMapTexture(game.Content, "movementmap", false, false), new Vector2(250, 450))
		{
			bg = new Layer(game, this, CompositeTexture.NewSceneryTexture(game.Content, "bg", false, false));
			layers.Add(bg);
			layers.Add(new Layer(game, this, CompositeTexture.NewSceneryTexture(game.Content, "layer2", false, false)));
			layers.Add(new RoomLevel_Layer3(game, this));
		}

		protected override Vector2 UntiledLevelSize
		{
			get { return bg.UntiledLayerSize; }
		}
	}
}

