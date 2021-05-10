using System;
using System.Collections.Generic;
using System.Linq;
using ArcObjectsCodeGen.Runtime;
using EnsureThat;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace ArcObjectsCodeGen.Generators.Templates
{
	/// <summary>
	/// A parameter context for FeatureClass.tt template.
	/// </summary>
	public class FeatureClassTemplateContext
	{
		private readonly IClass m_Class;
		private readonly IDataset m_Dataset;
		private readonly ITable m_Table;
		private readonly IFeatureClass m_FeatureClass;
		private readonly FieldDefinition[] m_Fields;

		public FeatureClassTemplateContext(IClass tableOrFc)
		{
			m_Class = EnsureArg.IsNotNull(tableOrFc, nameof(tableOrFc));
			m_Dataset = (IDataset)m_Class;
			m_Table = (ITable)m_Class;
			m_FeatureClass = m_Class as IFeatureClass;
			m_Fields = FieldDefinition.Enumerate(m_Class.Fields).ToArray();
			// ????
			// field.Type != esriFieldType.esriFieldTypeOID) // to create table/fc, there is no need for Object ID
		}

		public string Namespace => $"{nameof(Runner)}.GeneratedCode";

		/// <summary>
		/// Name of C# class for table/feature class helpers.
		/// </summary>
		public string ClassName => $"{TableName}{MachineFriendlyDatasetType}";

		/// <summary>
		/// Name of C# class for data object class.
		/// </summary>
		public string DataObjectClassName => TableName;

		/// <summary>
		/// Name of table or feature class
		/// </summary>
		public string TableName => m_Dataset.Name.Split('.').Last(); // don't know how to get short name properly

		public esriDatasetType DatasetType => m_Dataset.Type;

		public string MachineFriendlyDatasetType => DatasetType switch
		{
			esriDatasetType.esriDTFeatureClass => "FeatureClass",
			esriDatasetType.esriDTTable => "Table",
			_ => throw new InvalidOperationException($"DatasetType={DatasetType}"),
		};

		public string HumanFriendlyDatasetType => DatasetType switch
		{
			esriDatasetType.esriDTFeatureClass => "Feature class",
			esriDatasetType.esriDTTable => "Table",
			_ => throw new InvalidOperationException($"DatasetType={DatasetType}"),
		};

		public string InterfaceName => DatasetType switch
		{
			esriDatasetType.esriDTFeatureClass => nameof(IFeatureClass),
			esriDatasetType.esriDTTable => nameof(ITable),
			_ => throw new InvalidOperationException($"DatasetType={DatasetType}"),
		};

		public string CLSID_Construct => ToCtorExpression(m_Class.CLSID);

		public string EXTCLSID_Construct => ToCtorExpression(m_Class.EXTCLSID);

		public string FeatureTypeString => m_FeatureClass.FeatureType.ToString();

		public string ShapeFieldName => m_FeatureClass.ShapeFieldName;

		/// <summary>
		/// Specific geometry type: IPoint, etc...
		/// </summary>
		public string GeometryInterface => m_Fields.First(field => field.IsShape).GeometryInterface;

		public IEnumerable<FieldDefinition> FieldDefinitions => m_Fields;

		public string ObjectToLiteral(object obj)
		{
			if (obj.Equals(DBNull.Value))
				return "DBNull.Value";

			return CSharpSyntaxHelpers.ObjectToLiteral(obj);
		}

		/// <summary>
		/// Returns spatial reference of the feature class serialized as string that can be deserialized at runtime.
		/// </summary>
		public string SerializedSpatialReference
		{
			get
			{
				// Get spatial reference
				int shapeFieldIdx = m_Class.Fields.FindField(ShapeFieldName);
				IField shapeField = m_Class.Fields.Field[shapeFieldIdx];
				var spatialReference = shapeField.GeometryDef.SpatialReference;

				// Get coclass GUID so we know what SR coclass to restore (ProjectedCoordinateSystem, GeographicCoordinateSystem)
				var srPresistStream = (ISRPersistStream)spatialReference;
				srPresistStream.GetClassID(out Guid coClassID);

				return AoSerializationHelpers.SerializeViaIPersistStream(spatialReference, coClassID);
			}
		}

		#region Implementation

		private static string ToCtorExpression(UID uid)
		{
			if (uid is null)
				return "(UID)null";

			return $"new UID {{ Value = \"{uid.Value}\", SubType = {uid.SubType} }}";
		}

		#endregion
	}
}
