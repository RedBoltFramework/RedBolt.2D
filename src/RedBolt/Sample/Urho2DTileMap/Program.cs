// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Urho2DTileMap;

using System;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Graphics2D;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.Resource;
using RedBolt.Scene;
using RedBolt.Scene2D;

internal static class Urho2DTileMapProgram
{
    private static int Main()
    {
        try { using var application = new Urho2DTileMap(); return application.Run(); }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}

/// <summary>Demonstrates loading and rendering an isometric TMX map.</summary>
public sealed class Urho2DTileMap : Application
{
    private Node? _cameraNode;

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Urho2DTileMap";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.2D;SampleData.2D;CoreData;SampleData";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
        parameters.WindowHeight = 720;
        parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Urho2D Tile Map";
    }

    protected override void Start()
    {
        Scene2D.RegisterUrho2DLibrary(Context);
        var scene = new Scene();
        scene.CreateComponent<Octree>();
        var zone = scene.CreateChild("Backdrop").CreateComponent<Zone>();
        zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
        zone.SetFogColor(new Color(0.23f, 0.23f, 0.23f));
        _cameraNode = scene.CreateChild("Camera");
        _cameraNode.SetPosition(new Vector3f(0.0f, 0.0f, -10.0f));
        var camera = _cameraNode.CreateComponent<Camera>();
        camera.SetOrthographic(true);
        camera.OrthoSize = Graphics.Height * Sprite2D.PixelSize;

        if (Resources.Get<TmxFile2D>("Urho2D/Tilesets/TiledSample/Sample.tmx") is { } tmx)
        {
            var tileMap = scene.CreateChild("TileMap").CreateComponent<TileMap2D>();
            tileMap.TmxFile = tmx;
            var contentBounds = GetTileContentBounds(tmx);
            var center = contentBounds.Defined
                ? contentBounds.Center
                : new Vector2f(tileMap.Info.MapWidth, tileMap.Info.MapHeight) * 0.5f;
            _cameraNode.SetPosition(new Vector3f(center.X, center.Y, -10.0f));
            if (contentBounds.Defined && contentBounds.Size.X > 0.0f && contentBounds.Size.Y > 0.0f)
            {
                var fitZoom = MathF.Min(Graphics.Width * Sprite2D.PixelSize / contentBounds.Size.X,
                    Graphics.Height * Sprite2D.PixelSize / contentBounds.Size.Y);
                camera.SetZoom(fitZoom * 0.9f);
            }
        }
        Renderer.SetViewport(0, new Viewport(scene, camera));
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || Input.GetKeyPress(Key.Escape)) { Exit(); return; }
        const float speed = 4.0f;
        var movement = Vector2f.Zero;
        if (Input.GetKeyDown(Key.W)) movement.Y += speed * timeStep;
        if (Input.GetKeyDown(Key.S)) movement.Y -= speed * timeStep;
        if (Input.GetKeyDown(Key.A)) movement.X -= speed * timeStep;
        if (Input.GetKeyDown(Key.D)) movement.X += speed * timeStep;
        _cameraNode!.Translate2D(movement);
        var camera = _cameraNode.GetComponent<Camera>()!;
        if (Input.GetKeyDown(Key.PageUp)) camera.SetZoom(camera.Zoom * 1.01f);
        if (Input.GetKeyDown(Key.PageDown)) camera.SetZoom(camera.Zoom * 0.99f);
    }

    private static Rect GetTileContentBounds(TmxFile2D tmx)
    {
        var bounds = new Rect();
        for (uint layerIndex = 0; layerIndex < tmx.NumLayers; ++layerIndex)
        {
            if (tmx.GetLayer(layerIndex) is not TmxTileLayer2D { IsVisible: true } layer)
                continue;
            for (var y = 0; y < layer.Height; ++y)
            for (var x = 0; x < layer.Width; ++x)
            {
                var tile = layer.GetTile(x, y);
                if (tile?.Sprite is not { } sprite ||
                    !sprite.GetDrawRectangle(out var spriteBounds, tile.FlipX, tile.FlipY))
                    continue;
                var position = tmx.Info.TileIndexToPosition(x, y) + layer.Offset;
                spriteBounds.Min += position;
                spriteBounds.Max += position;
                bounds.Merge(spriteBounds);
            }
        }
        return bounds;
    }
}
