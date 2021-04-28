using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace ArcObjectsCodeGen.AoGenerators.Templates
{
	public class RowWrapper
	{
		private IRow m_Row;
		private FieldDefinition[] m_Fields;
		private string m_TableOrFCClassName;

		public RowWrapper(IRow row, string tableOrFCClassName)
		{
			m_Row = row;
			m_Fields = FieldDefinition.Enumerate(m_Row.Fields).ToArray();
			m_TableOrFCClassName = tableOrFCClassName;
		}

		public IEnumerable<FieldDefinition> Fields => m_Fields;

		public string GetValueInitCSharpString(FieldDefinition field)
		{
			var sb = new StringBuilder($"{field.Name} = ");

			int fieldIndex = m_Row.Fields.FindField(field.RawField.Name);
			object rawValue = m_Row.Value[fieldIndex];
			if (rawValue is null || DBNull.Value.Equals(rawValue))
			{
				sb.Append("null");
			}
			else if (rawValue is string)
			{
				string stringLiteral = CSharpSyntaxHelpers.ObjectToLiteral(rawValue);

				// Guid values are also read as strings
				if (field.RawField.Type == esriFieldType.esriFieldTypeGUID || field.RawField.Type == esriFieldType.esriFieldTypeGlobalID)
					sb.AppendFormat("new Guid({0})", stringLiteral);
				else
					sb.Append(stringLiteral);
			}
			else if (rawValue is int || rawValue is short || rawValue is long || rawValue is double || rawValue is float)
			{
				sb.Append(CSharpSyntaxHelpers.ObjectToLiteral(rawValue));
			}
			else if (rawValue is DateTime dt)
			{
				sb.Append($"new DateTime({dt.Ticks}, DateTimeKind.{dt.Kind}) /* {dt}*/");
			}
			else if (rawValue is Guid guid)
			{
				sb.Append($"new Guid(\"{guid}\")");
			}
			else if (rawValue is IGeometry geometry)
			{
				if (!BuildGeometryInitializier(sb, geometry))
					return $"// TODO: {field.Name} is of not supported geometry type"; ;
			}
			else
			{
				return $"// TODO: {field.Name} of type {rawValue.GetType().Name}";
			}

			return sb.ToString();
		}

		#region Implementation

		private bool BuildGeometryInitializier(StringBuilder sb, IGeometry geometry)
		{
			if (geometry is IPoint point)
				return BuildPointGeometryInitializier(sb, point);

			if (geometry is IPolyline polyline)
				return BuildPolylineGeometryInitializier(sb, polyline);

			if (geometry is IPolygon polygon)
				return BuildPolygonGeometryInitializier(sb, polygon);

			return false;
		}

		private bool BuildPointGeometryInitializier(StringBuilder sb, IPoint point)
		{
			// Declare inline anonymous function and call it immediately. This will allow us to write a complex initializier body inline.
			sb.Append("((Func<IPoint>)(() => {");

			sb.Append("IPoint pt = new PointClass");
			sb.Append("{");
			sb.AppendFormat("SpatialReference = {0}.OriginalSpatialReference,", m_TableOrFCClassName);
			sb.AppendFormat("X = {0},", CSharpSyntaxHelpers.ObjectToLiteral(point.X));
			sb.AppendFormat("Y = {0},", CSharpSyntaxHelpers.ObjectToLiteral(point.Y));
			sb.AppendFormat("Z = {0},", CSharpSyntaxHelpers.ObjectToLiteral(point.Z));
			sb.AppendFormat("M = {0}", CSharpSyntaxHelpers.ObjectToLiteral(point.M));
			sb.Append("};");
			sb.Append("return pt;");

			sb.Append("}))()");
			return true;
		}

		private bool BuildPolylineGeometryInitializier(StringBuilder sb, IPolyline polyline)
		{
			// Simplify work using binary serialization
			sb.
				Append(m_TableOrFCClassName).
				Append(".DeserializeViaIPersistStream<IPolyline>(\"").
				Append(AoHelpers.SerializeViaIPersistStream(polyline, typeof(PolylineClass).GUID)).
				Append("\")");
			return true;
		}

		private bool BuildPolygonGeometryInitializier(StringBuilder sb, IPolygon polygon)
		{
			var navaras = (IZAware)polygon;

			// Simplify work using binary serialization
			sb.
				Append(m_TableOrFCClassName).
				Append(".DeserializeViaIPersistStream<IPolygon>(\"").
				Append(AoHelpers.SerializeViaIPersistStream(polygon, typeof(PolygonClass).GUID)).
				Append("\")");
			return true;
		}

		#endregion
	}
}
