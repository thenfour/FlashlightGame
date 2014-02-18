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
		public TileLevel(PitchBlackGame game) :
			base(game, "TileMM", new Vector2(0, 0), new Vector2(2040,1218))
		{
			layers.Add(new Layer(game, this, "TileBG", true));
		}
	}
}

