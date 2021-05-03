using System;
using System.IO;
using System.Text;

namespace ArcObjectsCodeGen.Generators.Templates
{
	internal static class T4Templates
	{
		public static string FeatureClass => GetFromEmbeddedResource("FeatureClass.tt");

		public static string FeatureDO => GetFromEmbeddedResource("FeatureDO.tt");

		#region Implementation

		private static string GetFromEmbeddedResource(string fileName)
		{
			var type = typeof(T4Templates);
			var assembly = type.Assembly;

			string pathToEmbeddedRes = string.Concat(type.Namespace, ".", fileName);

			Stream resourceStream = assembly.GetManifestResourceStream(pathToEmbeddedRes);
			if (resourceStream == null)
				throw new ArgumentException(
					$"Resource stream {pathToEmbeddedRes} was not found in {assembly.FullName}");

			using (resourceStream)
			using (var resourceStreamReader = new StreamReader(resourceStream, Encoding.UTF8))
			{
				return resourceStreamReader.ReadToEnd();
			}
		}

		#endregion
	}
}
