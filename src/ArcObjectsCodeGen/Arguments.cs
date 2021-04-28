using System;
using System.Linq;
using ArcObjectsCodeGen.AoGenerators;

namespace ArcObjectsCodeGen
{
	/// <summary>
	/// Parsed program arguments
	/// </summary>
	internal class Arguments
	{
		public const string OutputToConsole = "$CON$";

		/// <summary>
		/// Task name defines what generator should do
		/// </summary>
		public string TaskName { get; set; }

		// We can use subclasses of Arguments depending on action.
		public string ConnectionFile { get; set; }
		public string NameOfTableOrFC { get; set; }
		public string Query { get; set; }
		public string OutputFolder { get; set; } = ".";

		public static void PrintUsage()
		{
			Logger.Error("Bad arguments. Look at source code and resubmit.");
		}

		/// <summary>
		/// Parses command line <see cref="args"/> and creates appropriate Arguments instance.
		/// </summary>
		public static Arguments FromCommandLine(string[] args)
		{
			if (args.Length < 3)
				return null;

			var tasks = AoRunner.GetAvailableTasks().ToArray();
			if (tasks.Contains(args[0]))
			{
				var result = new Arguments
				{
					TaskName = args[0],
					ConnectionFile = args[1],
					NameOfTableOrFC = args[2]
				};

				if (args.Length >= 4)
				{
					result.OutputFolder = args[3];
				}

				int queryIndex = Array.IndexOf(args, "--query");
				if (queryIndex >= 0)
				{
					if (args.Length < queryIndex + 2)
					{
						Logger.Error("Pass query after --query.");
						return null;
					}

					result.Query = args[queryIndex + 1];
				}

				return result;
			}

			return null;
		}
	}
}
