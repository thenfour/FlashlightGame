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
	///////////////////////////////////////////////////////////////////////////////////////////
	public class ShaderEffect
	{
		protected Effect effect;

		public ShaderEffect(ContentManager content, string effectFile)
		{
			effect = content.Load<Effect>(effectFile);
		}

		public virtual void Update(KeyboardState keyboardState, MouseState mouseState, GameTime gameTime)
		{
		}

		public virtual void Apply(Texture2D source, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
			effect.Begin();
			effect.CurrentTechnique.Passes[0].Begin();
			spriteBatch.Draw(source, Vector2.Zero, Color.White);
			effect.CurrentTechnique.Passes[0].End();
			effect.End();
			spriteBatch.End();
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	public class WaveEffect :
		ShaderEffect
	{
		EffectParameter waveParam;
		EffectParameter distortionParam;
		EffectParameter centerCoordParam;

		Vector2 centerCoord = new Vector2(0.5f);
		float distortion = 1.0f;
		float divisor = 0.75f;
		float wave = MathHelper.Pi;

		public WaveEffect(ContentManager content) :
			base(content, "Shaders\\Wave")
		{
			waveParam = effect.Parameters["wave"];
			distortionParam = effect.Parameters["distortion"];
			centerCoordParam = effect.Parameters["centerCoord"];

			centerCoord = new Vector2(0.5f);
			distortion = 1.0f;
			divisor = 0.75f;
			wave = MathHelper.Pi / divisor;
		}

		public override void Update(KeyboardState keyboardState, MouseState mouseState, GameTime gameTime)
		{
			base.Update(keyboardState, mouseState, gameTime);

			centerCoordParam.SetValue(centerCoord);
			distortionParam.SetValue(distortion);
			waveParam.SetValue(wave);
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	public class ZoomBlurEffect :
		ShaderEffect
	{
		EffectParameter center;
		EffectParameter amount;

		public ZoomBlurEffect(ContentManager content) :
			base(content, "Shaders\\ZoomBlur")
		{
			center = effect.Parameters["Center"];
			amount = effect.Parameters["BlurAmount"];

			center.SetValue(new Vector2(0.5f));
			amount.SetValue(0.02f);
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	public class PixelateEffect :
		ShaderEffect
	{
		EffectParameter HorizontalPixelCounts;
		EffectParameter VerticalPixelCounts;

		public PixelateEffect(ContentManager content) :
			base(content, "Shaders\\Pixelate")
		{
			HorizontalPixelCounts = effect.Parameters["HorizontalPixelCounts"];
			VerticalPixelCounts = effect.Parameters["VerticalPixelCounts"];

			HorizontalPixelCounts.SetValue(80);
			VerticalPixelCounts.SetValue(60);
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	public class PinchEffect :
		ShaderEffect
	{
		EffectParameter Center;
		EffectParameter Radius;
		EffectParameter Amount;

		public PinchEffect(ContentManager content) :
			base(content, "Shaders\\Pinch")
		{
			Center = effect.Parameters["Center"];
			Radius = effect.Parameters["Radius"];
			Amount = effect.Parameters["Amount"];

			Center.SetValue(new Vector2(0.5f));
			Radius.SetValue(2.0f);
			Amount.SetValue(-0.3f);
		}
	}

}
