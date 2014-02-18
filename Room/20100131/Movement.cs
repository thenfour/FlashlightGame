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
	public class LayerMap
	{
		public Texture2D movementmap;

		Color[] movementMapCodes =
		{
			new Color(0,0,0),// inv
			new Color(255,0,0),// layer 1 red
			new Color(0,0,255),// layer 2 blue
			new Color(0,255,0)// layer 3 green
		};

		int width;
		Color[] map;

		public LayerMap(Game game, string texture)
		{
			if (game == null)
				return;

			movementmap = game.Content.Load<Texture2D>(texture);
			width = movementmap.Width;
			map = new Color[movementmap.Height * movementmap.Width];
			movementmap.GetData(map, 0, map.Length);
		}

		// returns the movement layer based on the given coords.
		// returns 0 for invalid.
		public virtual int GetMovementLayer(int x, int y)
		{
			if (x < 0) return 0;
			if (y < 0) return 0;
			if (x >= width) return 0;

			int i = x + (y * width);
			if (i >= map.Length)
				return 0;
			Color playerMMColor = map[i];

			int ret = 0;
			bool found = false;
			foreach (Color c in movementMapCodes)
			{
				if (c == playerMMColor)
				{
					found = true;
					break;
				}
				ret++;
			}
			if (!found)
			{
				throw new Exception(string.Format("Layer map color {0} is unknown.", playerMMColor.ToString()));
			}
			return ret;
		}
		public virtual int GetMovementLayer(Vector2 pos)
		{
			return GetMovementLayer((int)pos.X, (int)pos.Y);
		}
	}

	public class TestLayerMap : LayerMap
	{
		string[] map = 
		{
			".......",
			".......",
			"xxxx...",
			".......",
			".......",
			"...xxxx",
			"...x...",
		};

		// a = starting point, b = ending point, c = expected ending point,
		// B = ending point + expected ending point in the same place.
		// A = start+expected end point
		string[][] tests = 
		{
			new string[]
			{
			  ".......",
			  ".......",
			  "xxxx...",
			  "...a...",
			  ".......",
			  "..cb...",
			  "......",
			},
			new string[]
			{//0123456
				".......",//0
				"...bc..",//1
				"xxxx...",//2
				".......",//3
				"...a...",//4
				"...x...",//5
				".......",//6
			},
			new string[]
			{//0123456
				".......",//0
				"b......",//1
				"xxxx...",//2
				"c......",//3
				"...a...",//4
				"...x...",//5
				".......",//6
			}
		};

		public TestLayerMap()
			: base(null, "movementmap")
		{
		}
		public override int GetMovementLayer(int x, int y)
		{
			if (x < 0) return 0;
			if (y < 0) return 0;
			if (x >= map[0].Length) return 0;
			if (y >= map.Length) return 0;
			switch (map[y][x])
			{
				case 'x':
				default:
					return 0;
				case '#':
				case '.':
					return 1;
			}
		}
		public int TestCount { get { return tests.Length; } }
		public void GetTestInfo(int i, out int startingX, out int startingY, out int destX, out int destY, out int expectedX, out int expectedY)
		{
			startingX = startingY = destX = destY = expectedX = expectedY = 0;
			string[] test = tests[i];
			for (int y = 0; y < test.Length; ++y)
			{
				string row = test[y];
				int a = row.IndexOf('a');
				if (a != -1)
				{
					startingX = a;
					startingY = y;
				}
				int b = row.IndexOf('b');
				if (b != -1)
				{
					destX = b;
					destY = y;
				}
				int c = row.IndexOf('c');
				if (c != -1)
				{
					expectedX = c;
					expectedY = y;
				}
				int B = row.IndexOf('B');
				if (B != -1)
				{
					destX = B;
					destY = y;
					expectedX = B;
					expectedY = y;
				}
				int A = row.IndexOf('A');
				if (A != -1)
				{
					startingX = A;
					startingY = y;
					expectedX = A;
					expectedY = y;
				}
			}
		}
	}

	public class MovementHelper
	{
		public Vector2 CurrentPosition = new Vector2();
		public const int tolerance = 8;

		// does not do some fancy pathfinding algo; this just makes sure you can move to this direction linearly.
		public bool CanMoveTo(Vector2 delta, LayerMap layerMap)
		{
			if (delta.Y != 0.0f)
			{
				int dir = delta.Y > 0 ? 1 : -1;
				for (int iy = dir; Math.Abs(iy) <= (int)Math.Abs(delta.Y); iy += dir)
				{
					int ix = (int)Math.Round(MathHelper.Lerp(0, delta.X, (float)iy / delta.Y));
					if (layerMap.GetMovementLayer((int)CurrentPosition.X + ix, (int)CurrentPosition.Y + iy) == 0)
						return false;
				}
				return true;
			}

			if (delta.X != 0.0f)
			{
				int dir = delta.X > 0 ? 1 : -1;
				for (int ix = dir; Math.Abs(ix) <= (int)Math.Abs(delta.X); ix += dir)
				{
					int iy = (int)Math.Round(MathHelper.Lerp(0, delta.Y, (float)ix / delta.X));
					if (layerMap.GetMovementLayer((int)CurrentPosition.X + ix, (int)CurrentPosition.Y + iy) == 0)
						return false;
				}
				return true;
			}

			return true;
		}

		private void AdjustPosition(Vector2 delta)
		{
			CurrentPosition.X += delta.X;
			CurrentPosition.Y += delta.Y;
		}

		/*

		-----
		-ada-
		-----
		
		*/

		public class Sampler : IDisposable
		{
			const int width = MovementHelper.tolerance * 2 + 1;
			const int height = MovementHelper.tolerance * 2 + 1;
			Vector2 org = new Vector2(MovementHelper.tolerance, MovementHelper.tolerance);

			int[] map = new int[width * height];

			public Sampler()
			{
				for(int i = 0; i < map.Length; ++i)
				{
					map[i] = 0;
				}
			}

			public void Sample(Vector2 delta)
			{
				Vector2 pos = org + delta;
				map[(int)pos.X + (width * (int)pos.Y)]++;
			}

			public void Dispose()
			{
				//System.Diagnostics.Debug.WriteLine("--------------------");
				//for (int y = 0; y < height; ++y)
				//{
				//  StringBuilder sb = new StringBuilder();
				//  for (int x = 0; x < width; ++x)
				//  {
				//    // 00o
				//    // 00x
				//    // 00!
				//    int val = map[x + (width * y)];
				//    sb.Append(val == 0 ? " -" : val.ToString("00"));
				//    if (y == org.Y && x == org.X)
				//    {
				//      sb.Append('*');
				//    }
				//    else
				//      sb.Append(' ');
				//  }
				//  System.Diagnostics.Debug.WriteLine(sb.ToString());
				//}
			}
		}

		// the BEST algorithm for this would be an astar
		// which starts at the start point, and travels towards the destination
		// very tightly, and only travels as far as the requested distance
		// then picks the position traveled which is the smallest dStart + dEnd and dTraveled (keeping it in a straight line, but traveling towards the dest)

		public void TryMove(Vector2 delta, LayerMap layerMap)
		{
			if (delta == Vector2.Zero)
				return;

			// start @ destination
			// outline concentric rectangles, traversing only 2 sides, towards the origin
			using (Sampler sampler = new Sampler())
			{
				float directionX = delta.X > 0 ? -1.0f : 1.0f;
				float directionY = delta.Y > 0 ? -1.0f : 1.0f;

				Vector2 newDelta = new Vector2();

				// +1 to make diagonal movement allow going slightly in the wrong direction.
				int biggestRadius = (int)Math.Min(1 + Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)), tolerance);

				// for diagonal movements, use a square model.
				if (delta.X != 0.0f && delta.Y != 0.0f)
				{
					for (int rectSize = 0; rectSize <= biggestRadius; ++rectSize)
					{
						for (int d2 = 0; d2 <= rectSize; ++d2)// and d2 traces the half-lines around that square
						{
							newDelta.X = delta.X + (directionX * rectSize);
							newDelta.Y = delta.Y + (directionY * d2);
							if (newDelta == Vector2.Zero)
								continue;
							sampler.Sample(newDelta);
							if (CanMoveTo(newDelta, layerMap))
							{
								AdjustPosition(newDelta);
								return;
							}

							if (d2 > 0)
							{
								newDelta.X = delta.X + (directionX * d2);
								newDelta.Y = delta.Y + (directionY * rectSize);
								sampler.Sample(newDelta);
								if (CanMoveTo(newDelta, layerMap))
								{
									AdjustPosition(newDelta);
									return;
								}
							}
						}
					}

					return;
				}

				float brd2 = (float)biggestRadius / 2;

				// for straight movements, use a diamond model. first start by growing larger, then smaller again.

				if (delta.Y != 0.0f)
				{
					// vertical movement
					for (float d1 = 0; d1 <= brd2; d1 += 1.0f)// vertical distance from delta, and half-diamond-width
					{
						float maxD2 = MathHelper.Lerp(0, tolerance, d1 / brd2);
						for (float d2 = 0; d2 <= maxD2; d2 += 1.0f)
						{
							newDelta.X = delta.X + (directionX * d2);
							newDelta.Y = delta.Y + (directionY * d1);
							sampler.Sample(newDelta);
							if (CanMoveTo(newDelta, layerMap))
							{
								AdjustPosition(newDelta);
								return;
							}

							if (d2 > 0)
							{
								newDelta.X = delta.X - (directionX * d2);
								newDelta.Y = delta.Y + (directionY * d1);
								sampler.Sample(newDelta);
								if (CanMoveTo(newDelta, layerMap))
								{
									AdjustPosition(newDelta);
									return;
								}
							}
						}
					}
					for (float d1 = brd2; d1 > 0; d1 -= 1.0f)// vertical distance from delta, and half-diamond-width
					{
						float maxD2 = MathHelper.Lerp(0, tolerance, d1 / brd2);
						for (float d2 = 0; d2 <= maxD2; ++d2)
						{
							newDelta.X = delta.X + (directionX * d2);
							newDelta.Y = delta.Y + (directionY * (d1 + brd2));
							sampler.Sample(newDelta);
							if (CanMoveTo(newDelta, layerMap))
							{
								AdjustPosition(newDelta);
								return;
							}

							if (d2 > 0)
							{
								newDelta.X = delta.X - (directionX * d2);
								newDelta.Y = delta.Y + (directionY * (d1 + brd2));
								sampler.Sample(newDelta);
								if (CanMoveTo(newDelta, layerMap))
								{
									AdjustPosition(newDelta);
									return;
								}
							}
						}
					}

					return;
				}

				// horizontal movement.
				// just copied from vertical movement, but with newDelta.X/Y switched and + and - switched
				for (float d1 = 0; d1 <= brd2; d1 += 1.0f)// horizontal distance from delta, and half-diamond-width
				{
					float maxD2 = MathHelper.Lerp(0, tolerance, d1 / brd2);
					for (float d2 = 0; d2 <= maxD2; d2 += 1.0f)
					{
						newDelta.Y = delta.Y + (directionY * d2);
						newDelta.X = delta.X + (directionX * d1);
						sampler.Sample(newDelta);
						if (CanMoveTo(newDelta, layerMap))
						{
							AdjustPosition(newDelta);
							return;
						}

						if (d2 > 0)
						{
							newDelta.Y = delta.Y - (directionY * d2);
							newDelta.X = delta.X + (directionX * d1);
							sampler.Sample(newDelta);
							if (CanMoveTo(newDelta, layerMap))
							{
								AdjustPosition(newDelta);
								return;
							}
						}
					}
				}
				for (float d1 = brd2; d1 > 0; d1 -= 1.0f)// horizontal distance from delta, and half-diamond-width
				{
					float maxD2 = MathHelper.Lerp(0, tolerance, d1 / brd2);
					for (float d2 = 0; d2 <= maxD2; ++d2)
					{
						newDelta.Y = delta.Y + (directionY * d2);
						newDelta.X = delta.X + (directionX * (d1 + brd2));
						sampler.Sample(newDelta);
						if (CanMoveTo(newDelta, layerMap))
						{
							AdjustPosition(newDelta);
							return;
						}

						if (d2 > 0)
						{
							newDelta.Y = delta.Y - (directionY * d2);
							newDelta.X = delta.X + (directionX * (d1 + brd2));
							sampler.Sample(newDelta);
							if (CanMoveTo(newDelta, layerMap))
							{
								AdjustPosition(newDelta);
								return;
							}
						}
					}
				}

				return;
			}
		}
	}
}
