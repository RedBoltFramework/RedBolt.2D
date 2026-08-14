# Building RedBolt.2D

Keep this repository beside RedBolt and RedBolt.Tests:

```text
RedBolt/
RedBolt.2D/
RedBolt.PhysicsBox2D/
RedBolt.Tests/
```

Build the Linux x64 rendering module and samples:

```sh
dotnet build RedBolt.2D.slnx --configuration Debug
```

The solution accepts the same platform, architecture, module, and sample selection properties as RedBolt. Its saved selection is independent and is written below `build/BuildConfig.props`.

Build the aggregate WebAssembly or Android sample hosts directly:

```sh
dotnet build src/RedBolt/Sample/Web/RedBolt.Sample.Web.csproj --configuration Release -p:RedBoltEnablePlatforms=web
dotnet build src/RedBolt/Sample/Android/RedBolt.Sample.Android.csproj --configuration Debug -f net10.0-android -p:RedBoltEnablePlatforms=android -p:RedBoltEnableArchitectures=all
```

Managed assemblies, sample outputs, and packages are written below this repository's `build/` directory. Base RedBolt outputs remain below `../RedBolt/build/`.
