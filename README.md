# ArcObjectsCodeGen
Generates C# code for ArcGIS geodatabase structure creation and data access objects in for in-memory unit tests.

You can put real data under unit test to verify your code.

You don't need a full integration test (well, leave it for the night build) and can run tests in memory (InMemoryWorkspaceFactoryClass).

## CLI command samples

`ArcObjectsCodeGen AO.FeatureClass ConnectionFile.sde FeatureClassName --output-folder generated-code`

Generates code to create a feature class.

`ArcObjectsCodeGen AO.Feature ConnectionFile.sde FeatureClassName --query "OBJECTID IN (1024, 1033)" --output-folder generated-code`

Generates feature samples.

## Notes

Just started, many TODO-s (most wanted: -namespace param).

Aquire ArcGIS license when running the test, release afterwards. You can use [this class](./src/ArcObjectsCodeGen/AoGenerators/ArcGisLicense.cs).

Run each test in STA thread. A single test will run fine in MTA but you may into issues when running multiple tests in parallel.
For XUnit there is [Xunit.StaFact](https://www.fuget.org/packages/Xunit.StaFact) by Andrew Arnott to solve this.

When running tests in STA thread avoid any ArcObjects COM object caching in static members as it may be created in another appartments. You'll get some COM interop exception.

## License
Apache License Version 2.0, see [LICENSE](./LICENSE). 
