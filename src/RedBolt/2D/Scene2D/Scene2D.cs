// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Scene2D;

using RedBolt.Core;

/// <summary>Entry point for scene-level 2D components.</summary>
public static class Scene2D
{
    public static void RegisterUrho2DLibrary(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.RegisterAssembly(typeof(Scene2D).Assembly);
    }
}
