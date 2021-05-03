using System;
using ESRI.ArcGIS.esriSystem;

namespace ArcObjectsCodeGen.Runtime
{
	public static class AoSerializationHelpers
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

		public static IAoInterface DeserializeViaIPersistStream<IAoInterface>(string serializedString)
		{
			// Divide serialized string into coclass Guid and serialized spatial reference bytes
			string[] parts = serializedString.Split('|');
			if (parts.Length != 2)
				throw new ArgumentException($"Bad format: {nameof(serializedString)}");

			var coClassID = new Guid(parts[0]);
			var itf = (IAoInterface)Activator.CreateInstance(Type.GetTypeFromCLSID(coClassID));
			var srPresistStream = (IPersistStream)itf;

			byte[] srBytes = Convert.FromBase64String(parts[1]);

			IMemoryBlobStream memoryStream = new MemoryBlobStreamClass();
			var memstreamVariant = (IMemoryBlobStreamVariant)memoryStream;
			memstreamVariant.ImportFromVariant(srBytes);

			srPresistStream.Load(memoryStream);

			return itf;
		}
	}
}
