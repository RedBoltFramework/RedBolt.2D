#:project ../../../RedBolt/src/RedBolt/Build/Packaging/RedBolt.ExternalModule.Packaging.csproj

using System.Runtime.CompilerServices;
using RedBolt.ExternalModule.Packaging;

return ExternalModulePackager.Run(args, new ExternalModulePackageManifest
{
    Name = "RedBolt.2D",
    BuildSteps =
    [
        new("2D/RedBolt.2D.csproj", Arguments: "-p:BuildProjectReferences=false")
    ],
    ResourcePackages =
    [
        new("RedBolt.2D.CoreData", "CoreData", "bin/CoreData"),
        new("RedBolt.2D.Data", "Data", "bin/Data")
    ],
    ManagedPackages =
    [
        new(
            "RedBolt.2D",
            "2D rendering with sprites particles Spriter animation and tile maps for RedBolt.",
            [new("build/bin/Release/linux-x64/core/RedBolt.2D.dll", "lib/net10.0/RedBolt.2D.dll")],
            [
                new("RedBolt.Core", PackageVersions.RedBolt),
                new("RedBolt.2D.CoreData", PackageVersions.Module),
                new("RedBolt.2D.Data", PackageVersions.Module)
            ])
    ],
    Consumer = new(
        "RedBolt.2D",
        """
        using System;
        using RedBolt.Scene2D;
        Console.WriteLine(typeof(StaticSprite2D).FullName);
        """)
}, SourcePath());

static string SourcePath([CallerFilePath] string path = "") => path;
