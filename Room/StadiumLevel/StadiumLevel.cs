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
	//////////////////////////////////////////////////////////////////
	public class StadiumLevel : Level
	{
		Layer bg;

		public StadiumLevel(PitchBlackGame game) :
			base(game, CompositeTexture.NewLayerMapTexture(game.Content, "StadiumMM", false, false), new Vector2(1480, 1066))
		{
			bg = new Layer(game, this, CompositeTexture.NewSceneryTexture(game.Content, "StadiumBG", false, false));
			layers.Add(bg);
		}

		protected override Vector2 UntiledLevelSize { get { return bg.UntiledLayerSize; } }
	}
}

