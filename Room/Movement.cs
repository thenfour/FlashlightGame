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
		CompositeTexture texture;

		Color[] movementMapCodes =
		{
			new Color(0,0,0),// inv
			new Color(255,0,0),// layer 1 red
			new Color(0,0,255),// layer 2 blue
			new Color(0,255,0)// layer 3 green
		};

		PitchBlackGame game;

		public void RefreshMode()
		{
			texture.RefreshMode();// allow us to use direct colors
			texture.InitColorMap();
		}

		public LayerMap(PitchBlackGame game, CompositeTexture texture)
		{
			this.game = game;
			this.texture = texture;
		}

		// returns the movement layer based on the given coords.
		// returns 0 for invalid.
		public virtual int GetMovementLayer(int x, int y)
		{
			// wrap x and y if necessary.
			if (texture.TileX)
			{
				x = Utility.WrapValue(0, texture.UntiledWidth, x);
			}
			if (texture.TileY)
			{
				y = Utility.WrapValue(0, texture.UntiledHeight, y);
			}

			if (x < 0) return 0;
			if (y < 0) return 0;
			if (x >= texture.UntiledWidth) return 0;
			if (y >= texture.UntiledHeight) return 0;
			Color playerMMColor = texture.GetPixel(x, y);

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
						if (CanMoveTo(newDelta, layerMap))
						{
							AdjustPosition(newDelta);
							return;
						}

						if (d2 > 0)
						{
							newDelta.X = delta.X + (directionX * d2);
							newDelta.Y = delta.Y + (directionY * rectSize);
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
						if (CanMoveTo(newDelta, layerMap))
						{
							AdjustPosition(newDelta);
							return;
						}

						if (d2 > 0)
						{
							newDelta.X = delta.X - (directionX * d2);
							newDelta.Y = delta.Y + (directionY * d1);
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
						if (CanMoveTo(newDelta, layerMap))
						{
							AdjustPosition(newDelta);
							return;
						}

						if (d2 > 0)
						{
							newDelta.X = delta.X - (directionX * d2);
							newDelta.Y = delta.Y + (directionY * (d1 + brd2));
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
					if (CanMoveTo(newDelta, layerMap))
					{
						AdjustPosition(newDelta);
						return;
					}

					if (d2 > 0)
					{
						newDelta.Y = delta.Y - (directionY * d2);
						newDelta.X = delta.X + (directionX * d1);
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
					if (CanMoveTo(newDelta, layerMap))
					{
						AdjustPosition(newDelta);
						return;
					}

					if (d2 > 0)
					{
						newDelta.Y = delta.Y - (directionY * d2);
						newDelta.X = delta.X + (directionX * (d1 + brd2));
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
