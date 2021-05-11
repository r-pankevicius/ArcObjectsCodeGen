using System;
using System.IO;
using System.Reflection;
using System.Text;
using EnsureThat;

namespace ArcObjectsCodeGen
{
	internal static class EmbeddedResourceReader
	{
		public static string GetUtf8Text(Type referenceType, string embeddedFileName)
		{
			EnsureArg.IsNotNull(referenceType, nameof(referenceType));

			return GetUtf8Text(referenceType.Assembly, referenceType.Namespace, embeddedFileName);
		}

		public static string GetUtf8Text(Assembly assembly, string @namespace, string embeddedFileName)
		{
			EnsureArg.IsNotNull(assembly, nameof(assembly));
			EnsureArg.IsNotNull(embeddedFileName, nameof(embeddedFileName));

			string pathToEmbeddedRes = string.IsNullOrEmpty(@namespace) ?
				embeddedFileName : // embedded at assembly root
				string.Concat(@namespace, ".", embeddedFileName);

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
	}
}
