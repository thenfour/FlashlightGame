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
	public class FlickerValues
	{
		float current;
		TimeSpan currentTime = TimeSpan.MinValue;
		float next;
		TimeSpan nextTime = TimeSpan.MinValue;

		Random rand = new Random();

		TimeSpan FlickLength = new TimeSpan(0, 0, 0, 0, 120);//1000 ms
		const float minVal = 0.70f;
		const float maxVal = 0.90f;
		const int flickerFreq = 150;

		public float GetNext(GameTime gameTime)
		{
			TimeSpan total = gameTime.TotalGameTime;

			if (currentTime == TimeSpan.MinValue)
			{
				// set up for the first time
				currentTime = total;
				current = (float)rand.NextDouble();
				next = (float)rand.NextDouble();
				nextTime = currentTime + FlickLength;
			}

			if (nextTime < total)
			{
				// advance to the next flicker
				current = next;
				currentTime = nextTime;
				next = (float)rand.NextDouble();
				nextTime = currentTime + FlickLength;
				while (nextTime < total)
				{
					nextTime += FlickLength;
				}
			}

			// randomly drop a flicker
			if (rand.Next(0, flickerFreq) == 0)
				return 0;

			// interpolate between current / next
			return minVal + ((maxVal - minVal) * MathHelper.Lerp(current, next, ((float)(total - currentTime).Ticks) / FlickLength.Ticks));
		}
	}

}
