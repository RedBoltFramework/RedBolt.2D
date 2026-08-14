# RedBolt.2D

Optional 2D rendering, sprites, particles, animation, and tile maps for [RedBolt](https://github.com/RedBoltFramework/RedBolt). Box2D physics lives in the sibling [RedBolt.PhysicsBox2D](https://github.com/RedBoltFramework/RedBolt.PhysicsBox2D) repository.

The source build expects RedBolt in `../RedBolt` by default. Override `RedBoltRoot` when the repositories use a different layout.

```sh
dotnet build RedBolt.2D.slnx --configuration Debug
```

Desktop, WebAssembly, and Android samples are included under `src/RedBolt/Sample`.

Tests remain in the sibling `RedBolt.Tests` repository. See [building](doc/BUILDING.md), [package production](packaging/nuget/README.md), and the [2D-specific deviations](doc/ALLOWED_DEVIATIONS.md).
