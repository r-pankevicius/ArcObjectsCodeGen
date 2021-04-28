using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace ArcObjectsCodeGen.AoGenerators.Templates
{
	public class FieldDefinition
	{
		internal const string ObjectIDFieldName = "ObjectID";
		internal const string ShapeFieldName = "Shape";

		private IField m_Field;

		public FieldDefinition(IField field)
		{
			m_Field = field;
		}

		internal IField RawField => m_Field;

		public string Name =>
			// Use uniform names for ObjectID and Shape because actual field names may differ
			// depending on what GDB type it is
			m_Field.Type switch
			{
				esriFieldType.esriFieldTypeOID => ObjectIDFieldName,
				esriFieldType.esriFieldTypeGeometry => ShapeFieldName,
				_ => m_Field.Name
			};

		public string AliasName => m_Field.AliasName;
		public string TypeString => m_Field.Type.ToString();
		public object DefaultValue => m_Field.DefaultValue;
		// TODO: public bool DomainFixed => m_Field.DomainFixed;
		// TODO: public IDomain Domain => m_Field.Domain;
		public bool IsEditable => m_Field.Editable;
		public bool IsNullable => m_Field.IsNullable;
		public bool IsRequired => m_Field.Required;
		public int Length => m_Field.Length;
		public int Precision => m_Field.Precision;
		public int Scale => m_Field.Scale;
		public GeometryDefinition GeometryDefinition => new(m_Field.GeometryDef);

		public bool IsObjectID => m_Field.Type == esriFieldType.esriFieldTypeOID;
		public bool IsShape => m_Field.Type == esriFieldType.esriFieldTypeGeometry;

		public string CSharpType
		{
			get
			{
				string NullableStructIfNeeded(string typeString) =>
					m_Field.IsNullable ? string.Concat(typeString, '?') : typeString;

				return m_Field.Type switch
				{
					esriFieldType.esriFieldTypeSmallInteger => NullableStructIfNeeded("short"),
					esriFieldType.esriFieldTypeInteger => NullableStructIfNeeded("int"),
					esriFieldType.esriFieldTypeSingle => NullableStructIfNeeded("float"),
					esriFieldType.esriFieldTypeDouble => NullableStructIfNeeded("double"),
					esriFieldType.esriFieldTypeString => "string",
					esriFieldType.esriFieldTypeDate => NullableStructIfNeeded("DateTime"),
					esriFieldType.esriFieldTypeOID => NullableStructIfNeeded("int"),
					esriFieldType.esriFieldTypeGeometry => GeometryInterface,
					esriFieldType.esriFieldTypeBlob => "byte[]",
					esriFieldType.esriFieldTypeRaster => "byte[]",
					esriFieldType.esriFieldTypeGUID => NullableStructIfNeeded("Guid"),
					esriFieldType.esriFieldTypeGlobalID => "Guid",
					esriFieldType.esriFieldTypeXML => "string",
					_ => throw new ArgumentException($"Unknown field type: {m_Field.Type}")
				};
			}
		}

		/// <summary>
		/// Specific geometry type: IPoint, etc...
		/// </summary>
		public string GeometryInterface
		{
			get
			{
				if (!IsShape)
					return "??Not Shape field??";

				return m_Field.GeometryDef.GeometryType switch
				{
					esriGeometryType.esriGeometryPoint => nameof(IPoint),
					esriGeometryType.esriGeometryPolyline => nameof(IPolyline),
					esriGeometryType.esriGeometryPolygon => nameof(IPolygon),
					_ => nameof(IGeometry) // This geometry type is not fully supported
				};
			}
		}

		public static IEnumerable<FieldDefinition> Enumerate(IFields fields)
		{
			int fieldCount = fields.FieldCount;
			for (int idxField = 0; idxField < fieldCount; idxField++)
			{
				var field = fields.Field[idxField];
				if (!field.Name.Contains(".")) // Ignore pseudo fields Shape.area. Shape.len
				{
					yield return new FieldDefinition(field);
				}
			}
		}
	}

	public class GeometryDefinition
	{
		private IGeometryDef m_GeometryDef;

		public GeometryDefinition(IGeometryDef geometryDef)
		{
			m_GeometryDef = geometryDef;
		}

		public bool IsValid => m_GeometryDef is not null;

		public string GeometryTypeString => m_GeometryDef.GeometryType.ToString();
	}

}
