using System;

namespace ArcObjectsCodeGen
{
	internal static class Logger
	{
		public static void Log(string message) => Console.WriteLine(message);
		public static void Log(string messageFormat, params object[] formatArgs) => Console.WriteLine(messageFormat, formatArgs);

		public static void Warn(string message)
		{
			using var _ = new SetConsoleForeColor(ConsoleColor.Yellow);
			Log(message);
		}

		public static void Warn(string messageFormat, params object[] formatArgs)
		{
			using var _ = new SetConsoleForeColor(ConsoleColor.Yellow);
			Log(messageFormat, formatArgs);
		}

		public static void Error(string message)
		{
			using var _ = new SetConsoleForeColor(ConsoleColor.Red);
			Log(message);
		}

		public static void Error(string messageFormat, params object[] formatArgs)
		{
			using var _ = new SetConsoleForeColor(ConsoleColor.Red);
			Log(messageFormat, formatArgs);
		}
	}

	internal class SetConsoleForeColor : IDisposable
	{
		ConsoleColor m_ForegroundColorBefore;

		public SetConsoleForeColor(ConsoleColor foregroundColor)
		{
			m_ForegroundColorBefore = Console.ForegroundColor;
			Console.ForegroundColor = foregroundColor;
		}

		public void Dispose() => Console.ForegroundColor = m_ForegroundColorBefore;
	}
}
