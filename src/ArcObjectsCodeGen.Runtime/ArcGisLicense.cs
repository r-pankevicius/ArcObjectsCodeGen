using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Linq;
using System.Threading;

namespace ArcObjectsCodeGen.Runtime
{
	public class ArcGisLicense : IDisposable
	{
		private ArcGisLicense(bool gotLicense)
		{
			GotLicense = gotLicense;
		}

		public bool GotLicense { get; }

		public void Dispose()
		{
			if (GotLicense)
				ReleaseLicense();
		}

		public static ArcGisLicense GetLicense()
		{
			ProductCode preferredProduct = (!Environment.Is64BitProcess) ? ProductCode.Desktop : ProductCode.Server;
			bool gotLicense = false;
			var thread = new Thread((ThreadStart)delegate
			{
				if (RuntimeManager.ActiveRuntime != null)
				{
					gotLicense = true;
				}
				else
				{
					RuntimeInfo[] source = RuntimeManager.InstalledRuntimes.ToArray();
					bool desktopInstalled = source.Any((RuntimeInfo r) => r.Product == ProductCode.Desktop);
					bool serverInstalled = source.Any((RuntimeInfo r) => r.Product == ProductCode.Server);

					ProductCode productCode;
					if (desktopInstalled && preferredProduct == ProductCode.Desktop)
					{
						productCode = ProductCode.Desktop;
					}
					else if (serverInstalled && preferredProduct == ProductCode.Server)
					{
						productCode = ProductCode.Server;
					}
					else if (desktopInstalled)
					{
						productCode = ProductCode.Desktop;
					}
					else
					{
						if (!serverInstalled)
						{
							return;
						}
						productCode = ProductCode.Server;
					}

					try
					{
						RuntimeManager.BindLicense(productCode);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.Print("Failed to bind license for {0} product.", productCode);
						System.Diagnostics.Debug.Print("{0}", ex);
						return;
					}

					IAoInitialize aoInitialize = new AoInitializeClass();
					esriLicenseStatus esriLicenseStatus = aoInitialize.Initialize((productCode == ProductCode.Server) ? esriLicenseProductCode.esriLicenseProductCodeArcServer : esriLicenseProductCode.esriLicenseProductCodeStandard);
					gotLicense = (esriLicenseStatus == esriLicenseStatus.esriLicenseCheckedOut || esriLicenseStatus == esriLicenseStatus.esriLicenseAlreadyInitialized);
				}
			});
			thread.Start();
			thread.Join();

			return new ArcGisLicense(gotLicense);
		}

		private static void ReleaseLicense()
		{
			IAoInitialize AoInitialize = new AoInitializeClass();
			AoInitialize.Shutdown();
		}
	}
}
