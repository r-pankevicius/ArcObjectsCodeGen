using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ArcObjectsCodeGen.AoGenerators.Templates;
using EnsureThat;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using Microsoft.VisualStudio.TextTemplating;
using Mono.TextTemplating;

namespace ArcObjectsCodeGen.AoGenerators
{
	/// <summary>
	/// Takes program input arguments and generates code for ArcObjects
	/// </summary>
	internal class AoRunner : IDisposable
	{
		const string GenerateFeatureClassTask = "AO.FeatureClass";
		const string GenerateFeatureDOSamplesTask = "AO.Feature";

		private readonly Arguments m_Arguments;
		private readonly Releaser m_Releaser;

		public AoRunner(Arguments arguments)
		{
			m_Arguments = EnsureArg.IsNotNull(arguments, nameof(arguments));
			m_Releaser = new();
		}

		public void Dispose() => m_Releaser.Dispose();

		public static IEnumerable<string> GetAvailableTasks() => new string[] { GenerateFeatureClassTask, GenerateFeatureDOSamplesTask };

		internal void Run()
		{
			if (m_Arguments.TaskName == GenerateFeatureClassTask)
			{
				GenerateFeatureClass();
			}
			else if (m_Arguments.TaskName == GenerateFeatureDOSamplesTask)
			{
				GenerateFillFeatureDO();
			}
			else
			{
				throw new ArgumentException($"Unknown task {m_Arguments.TaskName}.");
			}
		}

		#region Implementation

		private void GenerateFeatureClass()
		{
			IClass @class = OpenTableOrFeatureClass();
			if (@class is not null)
			{
				string template = T4Templates.FeatureClass;
				var context = new FeatureClassTemplateContext(@class);
				Generate(template, context, $"{context.ClassName}.cs");
			}
		}

		private void GenerateFillFeatureDO()
		{
			if (string.IsNullOrWhiteSpace(m_Arguments.Query))
			{
				Logger.Error("Query needs to be specified.");
				return;
			}

			IClass @class = OpenTableOrFeatureClass();
			if (@class is null)
				return;

			if (@class is not ITable table)
			{
				Logger.Error("Input is not table: needs to be a table or feature class to run a query.");
				return;
			}

			Logger.Log("Executing query `{0}`", m_Arguments.Query);

			IQueryFilter queryFilter = new QueryFilterClass
			{
				WhereClause = m_Arguments.Query
			};

			ICursor searchCursor = m_Releaser.ManageLifetime(table.Search(queryFilter, Recycling: false));
			IRow[] rows = ReadAllRows(searchCursor);
			if (!rows.Any())
			{
				Logger.Warn("The query has no results, no code will be generated.");
				return;
			}

			string template = T4Templates.FeatureDO;
			var context = new FeatureDOTemplateContext(table, rows);
			Generate(template, context, $"{context.ClassName}.cs");
		}

		private static IRow[] ReadAllRows(ICursor searchCursor)
		{
			// Hide the method, because it's not re-entrant
			static IEnumerable<IRow> EnumerateRows(ICursor searchCursor)
			{
				IRow row = searchCursor.NextRow();
				if (row is null)
					yield break;

				while (row is not null)
				{
					yield return row;
					row = searchCursor.NextRow();
				}
			}

			return EnumerateRows(searchCursor).ToArray();
		}

		/// <summary>
		/// Attempts to set ArcGIS license and open table or feature class using arguments.
		/// </summary>
		/// <returns>Not null if OK or null if failed to open</returns>
		IClass OpenTableOrFeatureClass()
		{
			var arguments = m_Arguments;

			Func<IWorkspace> openWorkspaceFunc = GetOpenWorkspaceFunction(arguments.ConnectionFile);
			if (openWorkspaceFunc is null)
			{
				Logger.Error("Can't open workspace from connection argument {0}", arguments.ConnectionFile);
				return null;
			}

			using var arcGisLicense = ArcGisLicense.GetLicense();
			if (!arcGisLicense.GotLicense)
			{
				Logger.Error("Could not get ArcGIS license.");
				return null;
			}

			Logger.Log("Opening workspace...");
			IWorkspace workspace = m_Releaser.ManageLifetime(openWorkspaceFunc());
			Logger.Log("Opened OK.");

			var featureWS = (IFeatureWorkspace)workspace;

			IClass @class;
			try
			{
				// Find out if it's table or feature class
				var ws2 = (IWorkspace2)workspace;
				if (ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, arguments.NameOfTableOrFC))
				{
					Logger.Log("Opening feature class {0}.", arguments.NameOfTableOrFC);
					@class = featureWS.OpenFeatureClass(arguments.NameOfTableOrFC);
				}
				else if (ws2.get_NameExists(esriDatasetType.esriDTTable, arguments.NameOfTableOrFC))
				{
					Logger.Log("Opening table {0}.", arguments.NameOfTableOrFC);
					@class = featureWS.OpenTable(arguments.NameOfTableOrFC);
				}
				else
				{
					Logger.Error("Neither table not feature class with this name was found in geodatabase: `{0}`.", arguments.NameOfTableOrFC);
					return null;
				}
			}
			catch (COMException ex) when (ex.HResult == (int)fdoError.FDO_E_TABLE_NOT_FOUND)
			{
				Logger.Error("{0}: `{1}`.", ex.Message, arguments.NameOfTableOrFC);
				return null;
			}

			return @class;
		}

		private void Generate(string template, object templateContext, string outpuFileName)
		{
			var generator = new TemplateGenerator();
			generator.Refs.Add(templateContext.GetType().Assembly.Location);

			ITextTemplatingSession templatingSession = generator.GetOrCreateSession();
			templatingSession["context"] = templateContext;

			GenerateAndWiteOutputFile(generator, template, outpuFileName);
		}

		private void GenerateAndWiteOutputFile(TemplateGenerator generator, string featureClassTemplate, string outputFileName)
		{
			bool success = generator.ProcessTemplate(
				inputFileName: null, inputContent: featureClassTemplate, ref outputFileName, out string outputContent);
			if (!success)
			{
				// Troubleshoot errors in debugger:
				// inspect generator errors, find a path to temporary file GeneratedTextTransformation.cs,
				// copy and add to the project under Temp folder to see compiler errors.
				Logger.Error("Failed to generate code. See comments in code how to troubleshoot this.");
				ShowErrors(generator);
				return;
			}

			Logger.Log("Code generated successfully.");

			if (m_Arguments.OutputFolder == "$CON$")
			{
				Logger.Log("");
				Logger.Log(outputContent);
			}
			else
			{
				if (!Directory.Exists(m_Arguments.OutputFolder))
				{
					Logger.Error("Output folder \"{0}\" doesn't exist.", m_Arguments.OutputFolder);
					return;
				}

				string outputFile = Path.Combine(m_Arguments.OutputFolder, outputFileName);
				Logger.Log("Writing output to \"{0}\".", outputFile);
				File.WriteAllText(outputFile, outputContent);
			}
		}

		/// <summary>
		/// Parses connection argument <paramref name="connectionArgument"/> and returns a function to open workspace.
		/// </summary>
		/// <param name="connectionArgument">Path to .sde file (TODO: other, like GDB folder)</param>
		/// <returns>Function to open the connection or null if it can't recognize or find <paramref name="connectionArgument"/>.</returns>
		private static Func<IWorkspace> GetOpenWorkspaceFunction(string connectionArgument)
		{
			EnsureArg.IsNotEmptyOrWhiteSpace(connectionArgument, nameof(connectionArgument));

			if (connectionArgument.EndsWith(".sde", StringComparison.OrdinalIgnoreCase))
			{
				string pathToSde = connectionArgument;

				if (File.Exists(pathToSde))
					return () => OpenBySdeconnectionFile(pathToSde);

					// Check path to sde
					if (!File.Exists(pathToSde))
				{
					if (pathToSde.Equals(Path.GetFileName(pathToSde), StringComparison.OrdinalIgnoreCase))
					{
						// Looking in: %USERPROFILE%\AppData\Roaming\ESRI\Desktop10.7\ArcCatalog folder (changing 10.7 to the current version)
						Version arcgisVersion = typeof(IWorkspace).Assembly.GetName().Version;
						string versionString = $"{arcgisVersion.Major}.{arcgisVersion.Minor}";

						string arcCatalogPath = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
							$@"ESRI\Desktop{versionString}\ArcCatalog",
							pathToSde);

						if (!File.Exists(arcCatalogPath))
						{
							Logger.Error("sde file doesn't exist. Checked \"{0}\" and \"{1}\"", pathToSde, arcCatalogPath);
							return null;
						}

						pathToSde = arcCatalogPath;
						Logger.Log("Will use input file \"{0}\".", arcCatalogPath);
						return () => OpenBySdeconnectionFile(pathToSde);
					}
					else
					{
						Logger.Error("sde file \"{0}\" doesn't exist.", pathToSde);
						return null;
					}
				}
			}

			return null;
		}

		// static string GetFullPath(string fileName) => Path.Combine(ExperimensFolder, fileName);
		private static IWorkspace OpenBySdeconnectionFile(string pathToSde)
		{
			EnsureArg.IsNotEmptyOrWhiteSpace(pathToSde, nameof(pathToSde));

			var factory = (IWorkspaceFactory)CreateComSingleton<SdeWorkspaceFactoryClass>();
			IWorkspace workspace = factory.OpenFromFile(pathToSde, hWnd: 0);
			return workspace;
		}

		public static object CreateComSingleton<T>()  => Activator.CreateInstance(Type.GetTypeFromCLSID(typeof(T).GUID));

		static void ShowErrors(TemplateGenerator generator)
		{
			if (generator.Errors.HasErrors)
			{
				foreach (var error in generator.Errors)
					Logger.Error(error.ToString());
			}
		}

		#endregion
	}
}
