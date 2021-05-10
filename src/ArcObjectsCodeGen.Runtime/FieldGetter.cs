using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using System;

namespace ArcObjectsCodeGen.Runtime
{
	public class FieldGetter<T>
	{
		private readonly int m_FieldIndex;

		public FieldGetter(int fieldIndex)
		{
			m_FieldIndex = fieldIndex;
		}

		public T GetValue(IRowBuffer rowBuffer)
		{
			object rawValue = rowBuffer.get_Value(m_FieldIndex);
			if (DBNull.Value.Equals(rawValue))
				return default;

			if (rawValue is string str && typeof(T) == typeof(Guid))
			{
				rawValue = new Guid(str);
			}
			else if (rawValue is IClone clone)
			{
				rawValue = clone.Clone(); // avoid "recycling" issues
			}

			return (T)rawValue;
		}
	}
}
