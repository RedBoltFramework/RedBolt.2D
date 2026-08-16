// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Graphics2D;

using RedBolt.Core;

/// <summary>Entry point for the 2D graphics library.</summary>
public static class Graphics2D
{
    public static void RegisterGraphics2DLibrary(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.RegisterAssembly(typeof(Graphics2D).Assembly);
    }
}
