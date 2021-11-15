using System;

namespace ArcObjectsCodeGen.Runtime
{
	/// <summary>
	/// To support xUnit IClassFixture<ArcGisLicenseFixture>:
	/// <code>
	/// public class MyTestClass : IClassFixture<ArcGisLicenseFixture>
	/// {
	/// ...
	/// }
	/// </code>
	/// </summary>
	public class ArcGisLicenseFixture : IDisposable
	{
		readonly ArcGisLicense m_License;

		public ArcGisLicenseFixture()
		{
			m_License = ArcGisLicense.GetLicense();
		}

		public void Dispose() => m_License.Dispose();
	}
}
