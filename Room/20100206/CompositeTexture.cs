using System;
using System.Collections.Generic;
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
	// a layer may be composed of many tiles. this class bundles them together
	// NOTE: all tiles must be the same size, but do not need to be square.
	public class CompositeTexture
	{
		public const int blackLevel = 2;

		private ContentManager content;
		private string[] tileNames = null;

		private bool tileX = false;
		private bool tileY = false;
		private Vector2 parallaxFactors;

		private int untiledWidth = 0;
		private int untiledHeight = 0;
		private Vector2 untiledSize;
		private int tileWidth = 0;
		private int tileHeight = 0;

		private int tileColumnCount;
		private int tileRowCount;

		private Texture2D[] tiles = null;
		private Texture2D[] tilesBlack = null;

		private bool generateSilhouette = false;

		private Color[] colorMap = null;

		public override string ToString()
		{
			if (tileNames == null)
				return "Blank composite texture";
			if (tileNames.Length == 0)
				return "Empty composite texture";
			return tileNames[0];
		}

		public bool TileX { get { return tileX; } }
		public bool TileY { get { return tileY; } }
		public int UntiledWidth { get { return untiledWidth; } }
		public int UntiledHeight { get { return untiledHeight; } }
		public Vector2 UntiledSize { get { return untiledSize; } }

		private CompositeTexture(ContentManager content, int columns, string[] textureNames, bool generateSilhouette, bool tileX, bool tileY, Vector2 parallaxFactors)
		{
			this.content = content;
			this.tileNames = textureNames;

			this.parallaxFactors = parallaxFactors;
			this.tileX = tileX;
			this.tileY = tileY;
			this.generateSilhouette = generateSilhouette;

			this.tileColumnCount = columns;
			this.tileRowCount = textureNames.Length / tileColumnCount;

			if (textureNames.Length % tileColumnCount != 0)
				throw new Exception("Invalid number of tiles specified.");
		}

		public static CompositeTexture NewLayerMapTexture(ContentManager content, string textureName, bool tileX, bool tileY)
		{
			return new CompositeTexture(content, 1, new string[] { textureName }, false, tileX, tileY, Vector2.One);
		}

		public static CompositeTexture NewLayerMapTexture(ContentManager content, int columns, string[] textureNames, bool tileX, bool tileY)
		{
			return new CompositeTexture(content, columns, textureNames, false, tileX, tileY, Vector2.One);
		}

		public static CompositeTexture NewSceneryTexture(ContentManager content, string textureName, bool tileX, bool tileY)
		{
			return new CompositeTexture(content, 1, new string[] { textureName }, true, tileX, tileY, Vector2.One);
		}

		public static CompositeTexture NewSceneryTexture(ContentManager content, int columns, string[] textureNames, bool tileX, bool tileY)
		{
			return new CompositeTexture(content, columns, textureNames, true, tileX, tileY, Vector2.One);
		}

		// players are always between parallax textures, so there is never any lighting applied.
		public static CompositeTexture NewParallaxTexture(ContentManager content, string textureName, bool tileX, bool tileY, Vector2 parallaxFactors)
		{
			return new CompositeTexture(content, 1, new string[] { textureName }, false, tileX, tileY, parallaxFactors);
		}

		public static CompositeTexture NewParallaxTexture(ContentManager content, int columns, string[] textureNames, bool tileX, bool tileY, Vector2 parallaxFactors)
		{
			return new CompositeTexture(content, columns, textureNames, false, tileX, tileY, parallaxFactors);
		}

		public void RefreshMode()
		{
			// (re)loads the textures & prepares the silhouette layers.
			if (tileNames != null)
			{
				tiles = new Texture2D[tileNames.Length];
				if (generateSilhouette)
					tilesBlack = new Texture2D[tileNames.Length];
				for (int i = 0; i < tileNames.Length; ++i)
				{
					Texture2D tile = content.Load<Texture2D>(tileNames[i]);
					tiles[i] = tile;
					if (generateSilhouette)
					{
						Texture2D blackness = new Texture2D(tile.GraphicsDevice, tile.Width, tile.Height, tile.LevelCount, tile.TextureUsage, tile.Format);
						tilesBlack[i] = blackness;
						Color[] cs = new Color[tile.Width * tile.Height];
						tile.GetData(cs);
						for (int p = 0; p < cs.Length; ++p)
						{
							cs[p].R = blackLevel;
							cs[p].G = blackLevel;
							cs[p].B = blackLevel;
							// leave alpha as it is in the original texture.
						}
						blackness.SetData(cs);
					}
				}

				tileWidth = tiles[0].Width;
				tileHeight = tiles[0].Height;
				untiledWidth = tileWidth * tileColumnCount;
				untiledHeight = tileHeight * tileRowCount;
				untiledSize = new Vector2(untiledWidth, untiledHeight);
			}
		}

		public void DrawLighting(Vector2 worldCoordinates, ScrollingMovement2D scroller, SpriteBatch spriteBatch)
		{
			if (tilesBlack == null)
				return;
			_Draw(tilesBlack, worldCoordinates, scroller, spriteBatch);
		}

		public void DrawScenery(Vector2 worldCoordinates, ScrollingMovement2D scroller, SpriteBatch spriteBatch)
		{
			_Draw(tiles, worldCoordinates, scroller, spriteBatch);
		}

		static void CalculateTileIndex(float worldCoordF, int tileSize, int tileCount, float parallaxFactor, out int index, out int tileOffset)
		{
			int worldCoord = (int)Math.Round(worldCoordF * parallaxFactor);
			if (worldCoord < 0)
			{
				//    -100 -99    -51 -50             0
				//      a   b      c   d              
				//      |      -2      |      -1      |  tile width=50
				index = (worldCoord + 1) / tileSize;// a:-1 b:-1 c:-1 d:0
				index -= 1;// a:-2 b:-2 c:-2 d:-1
			}
			else
			{
				// world coordinate is positive.
				//      0             50  51     99  100
				//                     a   b      c   d              
				//      |      0       |       1      |  tile width=50
				index = worldCoord / tileSize;
			}
			tileOffset = worldCoord - (index * tileSize);// a:0 b:1 c:49 d:0
			index = Utility.WrapValue(0, tileCount, index);
			return;
		}

		private void _Draw(Texture2D[] textures, Vector2 worldCoordinates, ScrollingMovement2D scroller, SpriteBatch spriteBatch)
		{
			// "worldCoordinates" refers to the origin; if it's tiled then it will cover the entire screen in that direction.
			Point screenCoordinates = scroller.WorldToScreen(worldCoordinates);

			// determine the tile that will go in the upper-left of the screen, and where the upper-left is
			Rectangle dest;
			Rectangle viewportRect = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

			// calculate upper left & bottom right coords.
			dest.X = tileX ? 0 : screenCoordinates.X;
			dest.Width = tileX ? viewportRect.Width : untiledWidth;
			dest.Y = tileY ? 0 : screenCoordinates.Y;
			dest.Height = tileY ? viewportRect.Height : untiledHeight;

			// shrink these values to only include visible portions.
			dest = Rectangle.Intersect(dest, viewportRect);

			// determine the world coordinates for the screen area we want to start drawing
			Vector2 worldUpperLeft = scroller.ScreenToWorld(new Point(dest.Left, dest.Top));

			// now determine tile info for the upper left tile.
			int leftTileColumn;
			int leftTileOffset;
			int topTileRow;
			int topTileOffset;
			CalculateTileIndex(worldUpperLeft.X, tileWidth, tileColumnCount, parallaxFactors.X, out leftTileColumn, out leftTileOffset);
			CalculateTileIndex(worldUpperLeft.Y, tileHeight, tileRowCount, parallaxFactors.Y, out topTileRow, out topTileOffset);

			int row = topTileRow;
			for (int y = dest.Top; y < dest.Bottom; )
			{
				int column = leftTileColumn;
				int ySourceOffset = y == dest.Top ? topTileOffset : 0;
				Vector2 renderDest = new Vector2(0, y);// x is not set for now.
				Rectangle renderSource = new Rectangle(
					0,
					ySourceOffset,
					0,
					tileHeight - ySourceOffset);// x / width are not set.

				for (int x = dest.Left; x < dest.Right; )
				{
					renderDest.X = x;
					int xSourceOffset = x == dest.Left ? leftTileOffset : 0;
					renderSource.X = xSourceOffset;
					renderSource.Width = tileWidth - xSourceOffset;

					Texture2D tile = textures[column + (row * tileColumnCount)];

					spriteBatch.Draw(tile, renderDest, renderSource, Color.White);

					x += tileWidth - xSourceOffset;
					column++;
					if (column == tileColumnCount)
						column = 0;
				}

				y += tileHeight - ySourceOffset;
				row++;
				if (row == tileRowCount)
					row = 0;
			}
		}

		public void InitColorMap()
		{
			if (tiles == null)
				throw new Exception("RefreshMode must be called before you can initialize the color map.");
			colorMap = new Color[untiledHeight * untiledWidth];
			for (int col = 0; col < tileColumnCount; ++col)
			{
				for (int row = 0; row < tileRowCount; ++row)
				{
					Texture2D tile = tiles[col + (row * tileColumnCount)];
					int offset = col + (row * untiledWidth);
					int count = tile.Width * tile.Height;
					tiles[col + (row * tileColumnCount)].GetData<Color>(colorMap, offset, count);
				}
			}
		}

		public Color GetPixel(int x, int y)
		{
			if (colorMap == null)
				throw new Exception("You haven't initialized the color map.");
			return colorMap[x + (y * untiledWidth)];
		}
	}

}

