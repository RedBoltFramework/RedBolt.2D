// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Scene2D;

using RedBolt.Resource;
using RedBolt.Scene;

public static class Urho2DEvents
{
    public struct ParticlesEndEvent { public Node Node; public ParticleEffect2D Effect; }
    public struct ParticlesDurationEvent { public Node Node; public ParticleEffect2D Effect; }
}
