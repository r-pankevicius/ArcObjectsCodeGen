using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace ArcObjectsCodeGen.AoGenerators.Templates
{
	/// <summary>
	/// A parameter context for FeatureDO.tt template.
	/// </summary>
	public class FeatureDOTemplateContext
	{
		IEnumerable<IRow> m_Rows;
		ITable m_Table;
		IDataset m_Dataset;

		public FeatureDOTemplateContext(ITable table, IEnumerable<IRow> rows)
		{
			m_Table = EnsureArg.IsNotNull(table, nameof(table));
			m_Rows = EnsureArg.IsNotNull(rows, nameof(rows));
			m_Dataset = (IDataset)m_Table;
		}

		public string Namespace => $"{nameof(AoRunner)}.GeneratedCode";

		/// <summary>
		/// Name of C# class.
		/// </summary>
		public string ClassName => $"{TableName}_DOSamples";

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

		public IEnumerable<RowWrapper> Rows
		{
			get
			{
				var fcTemplateContext = new FeatureClassTemplateContext(m_Table);
				return m_Rows.Select(row => new RowWrapper(row, fcTemplateContext.ClassName));
			}
		}
	}
}
