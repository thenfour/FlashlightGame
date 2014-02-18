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
	//public class RoomLevel_Layer3 : Layer
	//{
	//  bool candleLit = true;
	//  Texture2D candle;
	//  FlickerValues flicker = new FlickerValues();
	//  KeyDownHelper candleToggler = new KeyDownHelper();

	//  public RoomLevel_Layer3(PitchBlackGame game, Level level)
	//    : base(game, level, "layer3", false)
	//  {
	//  }

	//  public override void Update(KeyboardState keyboardState, MouseState mouseState, LayerMap layerMap)
	//  {
	//    base.Update(keyboardState, mouseState, layerMap);
	//    candleToggler.Update(keyboardState.IsKeyDown(Keys.F2), delegate() { candleLit = !candleLit; });
	//  }

	//  public override void RenderLighting(SpriteBatch spriteBatch, GameTime gameTime)
	//  {
	//    base.RenderLighting(spriteBatch, gameTime);

	//    // render candle
	//    if (candleLit)
	//    {
	//      float fa = flicker.GetNext(gameTime);
	//      spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
	//      game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
	//      game.GraphicsDevice.RenderState.SourceBlend = Blend.BlendFactor;
	//      game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
	//      game.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
	//      game.GraphicsDevice.RenderState.BlendFactor = new Color(fa, fa, fa, fa);
	//      level.DrawSprite(candle, Vector2.Zero);
	//      spriteBatch.End();
	//    }
	//  }
	//  public override void RefreshMode()
	//  {
	//    base.RefreshMode();
	//    candle = game.Content.Load<Texture2D>("candle");
	//  }
	//}

	//////////////////////////////////////////////////////////////////////
	//public class RoomLevel : Level
	//{
	//  public RoomLevel(PitchBlackGame game) :
	//    base(game, "movementmap", new Vector2(250, 450), new Vector2(600,505))
	//  {
	//    layers.Add(new Layer(game, this, "bg", true));
	//    layers.Add(new Layer(game, this, "layer2", false));
	//    layers.Add(new RoomLevel_Layer3(game, this));
	//  }
	//}
}

