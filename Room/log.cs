using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;


namespace PitchBlack
{
	public class Log
	{
		public static int indent = 0;

		public static void Message(string s)
		{
			string msg = s.Replace('\r', ' ').Replace('\n', ' ');
			System.Text.StringBuilder indentStr = new System.Text.StringBuilder();
			for (int i = 0; i < indent; ++i)
			{
				indentStr.Append("  ");
			}

			string entryWithoutTime = string.Format("{0}{1}",
				indentStr,
				msg
				);

			// add the date/time
			string entryWithTime = string.Format("[{0}] {1}",
				DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				entryWithoutTime
				);

			// log file
			string fileName = System.Reflection.Assembly.GetEntryAssembly().Location + ".log";

			System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, true, System.Text.Encoding.Unicode);
			sw.WriteLine(entryWithTime);
			sw.Close();

			// debug output
			System.Diagnostics.Debug.WriteLine(entryWithTime);

			// text box
		}

		public static void Message(string format, params object[] args)
		{
			Message(string.Format(format, args));
		}

		static int ExceptionLoggingDepth = 0;
		public static void Message(Exception ex, string context)
		{
			if (ExceptionLoggingDepth > 10)
				return;
			ExceptionLoggingDepth++;

			Message("{0}", ex.GetType().Name);

			if (!string.IsNullOrEmpty(context))
				Message("Context: " + context);

			if (!string.IsNullOrEmpty(ex.Message))
				Message("Message: " + ex.Message);

			if (!string.IsNullOrEmpty(ex.Source))
				Message("Source: " + ex.Source);

			if (!string.IsNullOrEmpty(ex.StackTrace))
			{
				Message("Stack trace:");
				indent++;
				string stack = ex.StackTrace.Replace('\r', '\n');
				string[] stackFrames = stack.Split('\n');

				foreach (string stackFrame in stackFrames)
				{
					if (string.IsNullOrEmpty(stackFrame))
						continue;
					Message(stackFrame);
				}
				indent--;
			}

			// stack trace is a multi-line thing.
			if (ex.InnerException != null)
			{
				Message(ex.InnerException);
			}

			ExceptionLoggingDepth--;
		}

		public static void Message(Exception ex)
		{
			Message(ex, null);
		}

		public static void Message(Exception ex, string format, params object[] args)
		{
			Message(ex, string.Format(format, args));
		}

		// must be used with using() in order to ensure proper destruction.
		public class ScopeMessage : IDisposable
		{
			bool open = false;
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

			public ScopeMessage(string format, params object[] args)
			{
				Message("{ " + string.Format(format, args));
				indent++;
				open = true;
				timer.Start();
			}
			public ScopeMessage(string msg)
			{
				Message("{ " + msg);
				indent++;
				open = true;
				timer.Start();
			}
			public ScopeMessage()
			{
				Message("{");
				indent++;
				open = true;
				timer.Start();
			}
			~ScopeMessage()
			{
				Dispose();
			}
			public void Dispose()
			{
				if (open)
				{
					timer.Stop();
					string timeElapsed = ((double)timer.ElapsedMilliseconds / 1000).ToString("0.000");

					indent--;
					Message("}} {0} seconds", timeElapsed);
				}
				open = false;
			}
		}
	}
}

