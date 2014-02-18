/*
	the super mario world solution is that you have a direction you're scrolling in,
	and the game tries to put a lot of screen in front of you (say 60% of the screen).
	to do this, when you switch directions, when you get to a certain threshold on the screen,
	it will slide you to that 40% position.

	so you start out at say 40% on the screen, in the left direction:
	|           <                 |
	|..;..;..;..;..|..;..;..;..;..|

	now you turn right, and move until the threshold, let's say 60%. the screen has not moved.
	|                 >           |
	|..;..;..;..;..|..;..;..;..;..|

	now you nudge right again, and the screen will auto-pan you like this:
				|           >                 |
				|..;..;..;..;..|..;..;..;..;..|

	from here, scrolling will sync with you as long as you continue in this direction.
	SMW also has the triggers which extend that visibility further.
				      |     >                       |
				      |..;..;..;..;..|..;..;..;..;..|

	new super mario bros i think does the same thing, but softens the movement in a cubic way.
	
	FOR US,
	we need to modify this slightly, to accommodate the idea that you may be moving
	in a direction you are not looking in. so we have AIM_POS + PLAYER_POS to take into account,
	and your direction is always dictated by the aim direction.

*/

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
	// needs to be instantiated any time the level changes or the player gets transported.
	// there is an assumption that the viewport & player position are 1:1, no zooming is involved.
	// so i sometimes mix the coordinates.
	public class ScrollingMovement2D
	{
		//////////////////////////////////////
		class Dimension
		{
			bool enableLog;

			/*
				when you are trying to aim at an object that's really close to the player, you will find that
				you may keep autopanning everywhere unless you're really precise. this means you should leave
				some room between min & max, so that you have a bit of leeway before switching directions
			*/
			public const float minViewportVisibility = 0.23f;// when do we start auto-panning?
			public const float maxViewportVisibility = 0.65f;// when do we stop auto-panning? when we get to this point, we set the direction too.
			public const float autoPanDuration = 800.0f;// auto scrolling duration, in milliseconds.

			public enum Direction
			{
				Indeterminate = 0,
				Descending = 1,
				Ascending = 2
			}

			bool autoPanning = false;// when true, we are aiming to get the player panned correctly
			float playerScreenPositionWhenAutoPanningStarted = 0.0f;// used for interpolating.
			float gameTimeWhenAutoPanningStarted;// milliseconds

			Direction currentDirection = Direction.Indeterminate;// direction the player is aiming, used for auto-panning and sync'd scrolling.

			public float currentScrollPos;// position on the level where the viewport starts

			public Dimension(float currentPlayerPos, float viewportSize, float levelSize, bool enableLog, bool InfiniteSize)
			{
				this.enableLog = enableLog;

				// center the player.
				currentScrollPos = currentPlayerPos - (viewportSize / 2);
				ClampScrollPos(viewportSize, levelSize, InfiniteSize);
			}

			/*
				take aim into account. the player position is only there to attempt to keep it in view.
			*/
			public void UpdateWithAim(float currentPlayerPos, float viewportSize, float untiledLevelSize, bool InfiniteSize, GameTime gameTime, float currentAimPos)
			{
				float currentPlayerScreenPos = currentPlayerPos - this.currentScrollPos;
				float currentAimScreenPos = currentAimPos - this.currentScrollPos;

				// START / RESTART AUTOPAN?
				float leftThreshold = viewportSize * minViewportVisibility;// number of pixels on the "low" side
				// we can only start autopanning left if you are not already going left.
				if ((currentDirection != Direction.Descending) && (currentAimScreenPos <= leftThreshold))
				{
					// start / restart an auto panning movement to the left?
					if (enableLog)
					{
						Log.Message(string.Format("Autopanning started LEFT"));
					}
					autoPanning = true;
					currentDirection = Direction.Descending;
					playerScreenPositionWhenAutoPanningStarted = currentPlayerScreenPos;
					gameTimeWhenAutoPanningStarted = (float)gameTime.TotalGameTime.TotalMilliseconds;
				}
				else if ((currentDirection != Direction.Ascending) && (currentAimScreenPos >= (viewportSize - leftThreshold)))
				{
					if (enableLog) Log.Message(string.Format("Autopanning started RIGHT"));
					autoPanning = true;
					currentDirection = Direction.Ascending;
					playerScreenPositionWhenAutoPanningStarted = currentPlayerScreenPos;
					gameTimeWhenAutoPanningStarted = (float)gameTime.TotalGameTime.TotalMilliseconds;
				}


				float idealPlayerScreenPosition = (currentDirection == Direction.Ascending) ?
					(viewportSize * (1.0f - maxViewportVisibility)) :// we need the player to be pushed to the left, so more visibility on the right.
					(viewportSize * (maxViewportVisibility));

				// PERFORM AUTO-PAN MOVEMENT
				if (autoPanning)
				{
					float autoPanPosition = ((float)gameTime.TotalGameTime.TotalMilliseconds - this.gameTimeWhenAutoPanningStarted) / autoPanDuration;// where are we in the auto-pan operation (0-1.0)
					// interpolate between current & target
					float newPlayerScreenPosition = MathHelper.SmoothStep(playerScreenPositionWhenAutoPanningStarted, idealPlayerScreenPosition, autoPanPosition);

					float oldScrollPos = currentScrollPos;
					currentScrollPos -= newPlayerScreenPosition - currentPlayerScreenPos;// scroll in the opposite direction the player appears to be moving

					if (enableLog)
					{
						ClampScrollPos(viewportSize, untiledLevelSize, InfiniteSize);
						Log.Message(string.Format("Autopanning ({0}) scrollPos:{1}->{2} playerScreen:{3}->{4}->{5}",
							autoPanPosition.ToString("0.00"),
							oldScrollPos.ToString("0"),
							currentScrollPos.ToString("0"),
							currentPlayerScreenPos.ToString("0"),
							newPlayerScreenPosition.ToString("0"),
							idealPlayerScreenPosition.ToString("0")
							));
					}

					// STOP AUTO-PANNING
					if (autoPanPosition >= 1.0f)
					{
						if (enableLog) Log.Message(string.Format("Autopanning stopped."));
						autoPanning = false;
					}
				}
				else
				{
					// sync'd scrolling. when scrolling is driven by aim, 
					// we sync up the player's position tightly.
					currentScrollPos = currentPlayerPos - idealPlayerScreenPosition;
				}

				ClampScrollPos(viewportSize, untiledLevelSize, InfiniteSize);
			}

			void ClampScrollPos(float viewportSize, float untiledLevelSize, bool InfiniteSize)
			{
				if (InfiniteSize)
					return;
				if (currentScrollPos < 0)
				{
					currentScrollPos = 0;
					return;
				}
				float maxScroll = Math.Max(0, untiledLevelSize - viewportSize);
				if (currentScrollPos > maxScroll)
				{
					currentScrollPos = maxScroll;
					return;
				}
			}

		}
		//////////////////////////////////////

		Dimension vert;
		Dimension horiz;

		public ScrollingMovement2D(Vector2 currentPlayerPos, Viewport viewport, Vector2 levelSize, bool InfiniteSizeX, bool InfiniteSizeY)
		{
			horiz = new Dimension(currentPlayerPos.X, viewport.Width, levelSize.X, false, InfiniteSizeX);
			vert = new Dimension(currentPlayerPos.Y, viewport.Height, levelSize.Y, false, InfiniteSizeY);
		}

		public float X
		{
			get { return horiz.currentScrollPos; }
		}

		public float Y
		{
			get { return vert.currentScrollPos; }
		}

		public Point WorldToScreen(Vector2 world)
		{
			return new Point((int)Math.Round(world.X - X), (int)Math.Round(world.Y - Y));
		}

		public Vector2 ScreenToWorld(Point screen)
		{
			return new Vector2(X + screen.X, Y + screen.Y);
		}

		public void Update(Vector2 currentPlayerPos, Vector2 currentAimPos, Viewport viewport, Vector2 untiledLevelSize, bool InfiniteSizeX, bool InfiniteSizeY, GameTime gameTime)
		{
			horiz.UpdateWithAim(currentPlayerPos.X, viewport.Width, untiledLevelSize.X, InfiniteSizeX, gameTime, currentAimPos.X);
			vert.UpdateWithAim(currentPlayerPos.Y, viewport.Height, untiledLevelSize.Y, InfiniteSizeY, gameTime, currentAimPos.Y);
		}
	}
}

