using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;

namespace ArcObjectsCodeGen.Runtime
{
	public class FieldSetter<T>
	{
		private readonly int m_FieldIndex;

		public FieldSetter(int fieldIndex)
		{
			m_FieldIndex = fieldIndex;
		}

		public void SetValue(IRowBuffer rowBuffer, T value)
		{
			object rawValue = value;
			if (rawValue is null)
			{
				rawValue = DBNull.Value;
			}
			else if (typeof(T) == typeof(Guid))
			{
				// TODO: specialized GUID getter/setter
				rawValue = (new Guid(value.ToString())).ToString("B").ToUpper();
			}
			else if (rawValue is IClone clone)
			{
				rawValue = clone.Clone(); // avoid "recycling" issues

				// Workaround for esri WTFs: COMException (0x80040907): Geometry cannot have Z values.
				// https://gis.stackexchange.com/a/269233/5834
				if (rawValue is IZAware zAwareRawValue && zAwareRawValue.ZAware && !rowBuffer.Fields.Field[m_FieldIndex].GeometryDef.HasZ)
					zAwareRawValue.ZAware = false;
			}

			rowBuffer.set_Value(m_FieldIndex, rawValue);
		}
	}
}
