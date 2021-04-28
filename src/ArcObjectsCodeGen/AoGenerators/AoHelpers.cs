using System;
using ESRI.ArcGIS.esriSystem;

namespace ArcObjectsCodeGen.AoGenerators
{
	internal static class AoHelpers
	{
		public static string SerializeViaIPersistStream(object objectToSerialize, Guid coClassID)
		{
			if (objectToSerialize is null)
				throw new ArgumentNullException(nameof(objectToSerialize));
			if (objectToSerialize is not IPersistStream persistStream)
				throw new ArgumentException($"nameof(objectToSerialize) doesn't implement IPersistStream.");

			// Persist object to the byte stream
			IMemoryBlobStream memoryStream = new MemoryBlobStreamClass();
			persistStream.Save(memoryStream, fClearDirty: 1);

			// Export IMemoryBlobStream to byte array
			var memStreamVariant = (IMemoryBlobStreamVariant)memoryStream;
			memStreamVariant.ExportToVariant(out object oSerializedBytes);
			byte[] serializedBytes = (byte[])oSerializedBytes;

			// Pack into coclass_GUID|serialized_sr_as_base64 string ('|' separator is not used in base 64, see https://base64.guru/learn/base64-characters)
			string serializedString = string.Concat(coClassID.ToString(), "|", Convert.ToBase64String(serializedBytes));
			return serializedString;
		}
	}
}
