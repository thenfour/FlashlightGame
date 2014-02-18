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
	public class TileLevel : Level
	{
		Layer bg;

		public TileLevel(PitchBlackGame game) :
			base(game, CompositeTexture.NewLayerMapTexture(game.Content, "50\\50red", true, true), new Vector2(25, 25))
		{
			bg = new Layer(game, this, CompositeTexture.NewSceneryTexture(game.Content, 3, new string[]
			{
				"50\\50blue",
				"50\\50brown",
				"50\\50cyan",
				"50\\50gray",
				"50\\50green",
				"50\\50magenta",
				"50\\50orange",
				"50\\50purple",
				"50\\50red",
			}, true, true));

			layers.Add(bg);
		}

		protected override bool InfiniteSizeX { get { return true; } }
		protected override bool InfiniteSizeY { get { return true; } }
		protected override Vector2 UntiledLevelSize { get { return bg.UntiledLayerSize; } }
	}
}

