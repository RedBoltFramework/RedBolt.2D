# RedBolt.2D workspace instructions

This repository is intentional RedBolt development. It contains the optional 2D rendering module extracted from the sibling RedBolt repository.

- RedBolt is expected at `../RedBolt` unless `RedBoltRoot` is supplied.
- Tests remain in `../RedBolt.Tests`.
- Preserve the public `RedBolt.2D` assembly and namespaces.
- Keep desktop, WebAssembly, and Android builds working.
- Build the smallest affected project and run the focused Graphics2D tests after implementation changes.
- At completion, build the solution, build Web and Android samples, create packages, run focused tests, and review a captured sample screenshot.
