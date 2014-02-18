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

		public static RenderTarget2D CloneRenderTarget(GraphicsDevice device)
		{
			return new RenderTarget2D(device,
					device.PresentationParameters.BackBufferWidth,
					device.PresentationParameters.BackBufferHeight,
					1,
					device.DisplayMode.Format,
					device.PresentationParameters.MultiSampleType,
					device.PresentationParameters.MultiSampleQuality
			);
		}

		public static int Clamp(int lowerBoundInclusive, int width, int val)
		{
			if (val < lowerBoundInclusive)
				return lowerBoundInclusive;
			int max = lowerBoundInclusive + width;
			if (val >= max)
				return max - 1;
			return val;
		}

		public static int WrapValue(int leftBoundaryInclusive, int width, int val)
		{
			while(val < leftBoundaryInclusive)
			{
				val += width;
			}
			int max = leftBoundaryInclusive + width;
			while (val >= max)
			{
				val -= width;
			}
			return val;
		}
	}
}
