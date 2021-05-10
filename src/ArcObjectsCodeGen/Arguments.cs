using System;
using System.Linq;
using ArcObjectsCodeGen.Generators;

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

			var tasks = Runner.GetAvailableTasks().ToArray();
			if (tasks.Contains(args[0]))
			{
				var result = new Arguments
				{
					TaskName = args[0],
					ConnectionFile = args[1],
					NameOfTableOrFC = args[2]
				};

				// --query SqlQuery
				{
					int queryArgIndex = Array.IndexOf(args, "--query");
					if (queryArgIndex >= 0)
					{
						if (args.Length < queryArgIndex + 2)
						{
							Logger.Error("Pass query after --query.");
							return null;
						}

						result.Query = args[queryArgIndex + 1];
					}
				}

				// --output-folder OutputFolder
				{
					int outputFolderArgIndex = Array.IndexOf(args, "--output-folder");
					if (outputFolderArgIndex >= 0)
					{
						if (args.Length < outputFolderArgIndex + 2)
						{
							Logger.Error("Pass output folder after --output-folder.");
							return null;
						}

						result.OutputFolder = args[outputFolderArgIndex + 1];
					}
				}

				return result;
			}

			return null;
		}
	}
}
