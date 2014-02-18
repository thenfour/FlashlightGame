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
	public class CourtyardLevel : Level
	{
		Layer bg;

		public CourtyardLevel(PitchBlackGame game) :
			base(game, CompositeTexture.NewLayerMapTexture(game.Content, "CourtyardMM", true, false), new Vector2(250, 450))
		{
			bg = new Layer(game, this, CompositeTexture.NewSceneryTexture(game.Content, "CourtyardBG", true, false));
			layers.Add(new Layer(game, this, CompositeTexture.NewParallaxTexture(game.Content, "CourtyardClouds", true, false, new Vector2(0.5f, 0.5f))));
			layers.Add(bg);
			layers.Add(new Layer(game, this, CompositeTexture.NewParallaxTexture(game.Content, "CourtyardSilhouette", true, false, new Vector2(1.5f, 1.5f))));
		}

		protected override bool InfiniteSizeX { get { return true; } }
		protected override Vector2 UntiledLevelSize
		{
			get { return bg.UntiledLayerSize; }
		}
	}
}

