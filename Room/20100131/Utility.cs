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
	public class KeyDownHelper
	{
		public delegate void OnClickHandler();

		bool wasPressed = false;
		public void Update(bool isPressedCurrently, OnClickHandler onclickEvent)
		{
			if (isPressedCurrently)
			{
				if (!wasPressed)
					onclickEvent();
				wasPressed = true;
			}
			else
				wasPressed = false;
		}
	}

	public class Utility
	{
		public const float ThreePiOver2 = MathHelper.PiOver2 * 3.0f;
	}
}
