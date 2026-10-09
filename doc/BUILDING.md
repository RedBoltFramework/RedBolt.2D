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

The solution accepts the same `RedBoltEnable`, module, and sample selection properties as RedBolt. Its saved selection is written to the active build root's `BuildConfig.props`: the module solution shares `build/BuildConfig.props` with the root solution, because both use the same build root. The build root is selected statically rather than recorded: every entry point (solution or direct project) uses the `build/` directory beside the engine directory (the monorepo `build/` in the monorepo), and `-p:RedBoltBuildRoot=<dir>` overrides it.

Build the aggregate WebAssembly or Android sample hosts directly:

```sh
dotnet build src/RedBolt/Sample/Web/RedBolt.Sample.Web.csproj --configuration Release -p:RedBoltEnable=linux-x64%2cbrowser-wasm
dotnet build src/RedBolt/Sample/Android/RedBolt.Sample.Android.csproj --configuration Debug -f net10.0-android -p:RedBoltEnable=linux-x64%2candroid
```

Outputs are written below the active build root. This is the `build/` directory beside the engine directory (the monorepo `build/` in the monorepo).
