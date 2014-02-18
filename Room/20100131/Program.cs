using System;

namespace PitchBlack
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			using (PitchBlackGame game = new PitchBlackGame())
			{
				game.Run();
			}
		}
	}
}

