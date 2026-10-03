# RedBolt.2D NuGet packages

RedBolt.2D versions independently from RedBolt, but every package records exact dependencies on the base RedBolt package version used for the build. Produce the matching base package set first in `<build root>/package/nuget/RedBolt/out` (`<build root>` is the monorepo `build/` inside the monorepo and the repository's own `build/` standalone; `REDBOLT_BUILD_ROOT` overrides it).

Create the rendering and resource packages from Release outputs, building the add-on first:

```sh
dotnet run --file packaging/nuget/package.cs -- create
```

Use `--no-build` when the managed output already exists. Override base version discovery only for an intentional local feed with `--redbolt-version VERSION`.

The command writes independently versioned packages below `<build root>/package/nuget/RedBolt.2D/out`:

- `RedBolt.2D`
- `RedBolt.2D.CoreData`
- `RedBolt.2D.SampleData`

Validate package contents, exact dependency versions, and a package-only consumer with:

```sh
dotnet run --file packaging/nuget/package.cs -- validate
```
