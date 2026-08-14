# Allowed deviations

These deviations apply to the 2D rendering module extracted from RedBolt:

- Urho2D is split into `Graphics2D` and `Scene2D` managed areas in the `RedBolt.2D` assembly. `Scene2D` depends only on `Graphics2D`; 2D physics lives in the independent `RedBolt.PhysicsBox2D` repository and assembly.
- The inactive compile-time Spine branch is not ported. The active Spriter/SCML path, including data loading, animation playback, character maps, and sprite lookup, is ported.
