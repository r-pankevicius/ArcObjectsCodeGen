using System;
using ArcObjectsCodeGen.AoGenerators;

namespace ArcObjectsCodeGen
{
	internal class Program
	{
		static int Main(string[] args)
		{
			try
			{
				// Hardcoded args order:
				// <command> ConnectionFile.sde NameOfTableOrFeatureClass [--query "<SQL query>"] [--output-folder optional_output_folder]
				// command: AO.FeatureClass OR AO.Feature
				// query example: "OBJECTID=5"
				var arguments = Arguments.FromCommandLine(args);
				if (arguments is null)
				{
					Arguments.PrintUsage();
					return 1;
				}

				using var aoRunner = new AoRunner(arguments);
				aoRunner.Run();
				return 0;
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
				return 2;
			}
		}
	}
}
