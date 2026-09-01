// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Urho2DSprite;

using System;
using System.Collections.Generic;
using RedBolt.Core;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Graphics2D;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.Resource;
using RedBolt.Scene;
using RedBolt.Scene2D;

internal static class Urho2DSpriteProgram
{
    private static int Main()
    {
        try { using var application = new Urho2DSprite(); return application.Run(); }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}

/// <summary>Demonstrates a small aquarium built from atlas sprites.</summary>
public sealed class Urho2DSprite : Application
{
    private SampleInputMap? _sampleInput;
    private const int NumFish = 80;
    private readonly List<(Node Node, StaticSprite2D Sprite, Vector2f MoveSpeed)> _sprites = [];
    private readonly List<(Node Node, Vector2f MoveSpeed)> _bubbles = [];
    private Node? _cameraNode;

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Urho2DSprite";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.2D;SampleData.2D;CoreData;SampleData";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
        parameters.WindowHeight = 720;
        parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Urho2D Sprite";
    }

    protected override void Start()
    {
        _sampleInput = new SampleInputMap(movement: true);
        _sampleInput.AddButton("ZoomIn", Scancode.PageUp, ControllerButton.RightShoulder, "+");
        _sampleInput.AddButton("ZoomOut", Scancode.PageDown, ControllerButton.LeftShoulder, "-", 1);
        Scene2D.RegisterUrho2DLibrary(Context);
        var scene = new Scene();
        scene.CreateComponent<Octree>();
        _cameraNode = scene.CreateChild("Camera");
        _cameraNode.SetPosition(new Vector3f(0.0f, 0.0f, -10.0f));
        var camera = _cameraNode.CreateComponent<Camera>();
        camera.SetOrthographic(true);
        camera.OrthoSize = Graphics.Height * Sprite2D.PixelSize;

        var fishSheet = Resources.Get<SpriteSheet2D>("Urho2D/Fish/spritesheet.xml");
        if (fishSheet is not null)
        {
            // Atlas entries touch each other, so keep bilinear filtering inside each 64px cell.
            foreach (var sprite in fishSheet.SpriteMapping.Values)
                sprite.TextureEdgeOffset = 1.0f;

            var halfWidth = Graphics.Width * 0.5f * Sprite2D.PixelSize;
            var halfHeight = Graphics.Height * 0.5f * Sprite2D.PixelSize;

            // Tile the blue water, then add a sandy floor and a water-surface edge.
            for (var x = -halfWidth + 0.32f; x < halfWidth; x += 0.64f)
            {
                for (var y = -halfHeight + 0.32f; y < halfHeight; y += 0.64f)
                    AddSprite(scene, fishSheet, "background_terrain", new Vector2f(x, y), -10);
                AddSprite(scene, fishSheet, "background_terrain_top", new Vector2f(x, halfHeight - 0.32f), -9);
                AddSprite(scene, fishSheet, "terrain_sand_a", new Vector2f(x, -halfHeight + 0.32f), -8);
                AddSprite(scene, fishSheet, "terrain_sand_top_a", new Vector2f(x, -halfHeight + 0.96f), -8);
            }

            var plants = new[] { "seaweed_green_a", "seaweed_green_b", "seaweed_orange_a", "seaweed_pink_a" };
            for (var i = 0; i < 12; ++i)
            {
                var x = -halfWidth + 0.5f + i * (Graphics.Width * Sprite2D.PixelSize / 12.0f);
                AddSprite(scene, fishSheet, plants[i % plants.Length], new Vector2f(x, -halfHeight + 1.25f), -6);
            }

            var rocks = new[] { "rock_a", "rock_b" };
            for (var i = 0; i < 6; ++i)
            {
                var x = -halfWidth + 0.8f + i * (Graphics.Width * Sprite2D.PixelSize / 6.0f);
                AddSprite(scene, fishSheet, rocks[i % rocks.Length], new Vector2f(x, -halfHeight + 0.9f), -5);
            }

            var bubbles = new[] { "bubble_a", "bubble_b", "bubble_c" };
            for (var i = 0; i < 18; ++i)
            {
                var x = MathDefs.Random(-halfWidth + 0.5f, halfWidth - 0.5f);
                var y = MathDefs.Random(-halfHeight + 1.6f, halfHeight - 0.5f);
                var node = AddSprite(scene, fishSheet, bubbles[i % bubbles.Length], new Vector2f(x, y), 1, 0.65f);
                if (node is not null)
                    _bubbles.Add((node, new Vector2f(MathDefs.Random(-0.08f, 0.08f), MathDefs.Random(0.25f, 0.55f))));
            }

            var fish = new[] { "fish_blue", "fish_green", "fish_orange", "fish_pink", "fish_red", "fish_brown" };
            for (var i = 0; i < NumFish; ++i)
            {
                var node = scene.CreateChild("Fish");
                node.SetPosition2D(MathDefs.Random(-halfWidth, halfWidth), MathDefs.Random(-halfHeight + 1.7f, halfHeight - 0.8f));
                var drawable = node.CreateComponent<StaticSprite2D>();
                drawable.Sprite = fishSheet.GetSprite(fish[i % fish.Length]);
                var speed = new Vector2f(MathDefs.Random(0.35f, 1.2f) * (i % 2 == 0 ? 1.0f : -1.0f), MathDefs.Random(-0.12f, 0.12f));
                drawable.FlipX = speed.X < 0.0f;
                _sprites.Add((node, drawable, speed));
            }
        }

        Renderer.SetViewport(0, new Viewport(scene, camera));
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || _sampleInput!.Pressed("Exit")) { Exit(); return; }
        MoveCamera(timeStep);
        var halfWidth = Graphics.Width * 0.5f * Sprite2D.PixelSize;
        var halfHeight = Graphics.Height * 0.5f * Sprite2D.PixelSize;
        for (var i = 0; i < _bubbles.Count; ++i)
        {
            var entry = _bubbles[i];
            var position = entry.Node.GetPosition2D() + entry.MoveSpeed * timeStep;
            if (position.Y > halfHeight - 0.5f)
            {
                position.X = MathDefs.Random(-halfWidth + 0.5f, halfWidth - 0.5f);
                position.Y = -halfHeight + 1.6f;
            }
            entry.Node.SetPosition2D(position);
            _bubbles[i] = entry;
        }

        for (var i = 0; i < _sprites.Count; ++i)
        {
            var entry = _sprites[i];
            var position = entry.Node.GetPosition2D() + entry.MoveSpeed * timeStep;
            var speed = entry.MoveSpeed;
            if (position.X < -halfWidth || position.X > halfWidth) speed.X = -speed.X;
            if (position.Y < -halfHeight + 1.7f || position.Y > halfHeight - 0.8f) speed.Y = -speed.Y;
            entry.Node.SetPosition2D(position);
            entry.Sprite.FlipX = speed.X < 0.0f;
            _sprites[i] = (entry.Node, entry.Sprite, speed);
        }
    }

    private static Node? AddSprite(Scene scene, SpriteSheet2D sheet, string name, Vector2f position, int layer, float scale = 1.0f)
    {
        if (sheet.GetSprite(name) is not { } sprite)
            return null;
        var node = scene.CreateChild(name);
        node.SetPosition2D(position);
        if (scale != 1.0f)
            node.SetScale2D(new Vector2f(scale, scale));
        var drawable = node.CreateComponent<StaticSprite2D>();
        drawable.Sprite = sprite;
        drawable.Layer = layer;
        return node;
    }

    private void MoveCamera(float timeStep)
    {
        const float moveSpeed = 4.0f;
        var movement = Vector2f.Zero;
        movement.Y += (_sampleInput!.Evaluate("Forward") - _sampleInput.Evaluate("Back")) * moveSpeed * timeStep;
        movement.X += (_sampleInput.Evaluate("Right") - _sampleInput.Evaluate("Left")) * moveSpeed * timeStep;
        _cameraNode!.Translate2D(movement);
        var camera = _cameraNode.GetComponent<Camera>()!;
        if (_sampleInput.Evaluate("ZoomIn") > 0.0f) camera.SetZoom(camera.Zoom * 1.01f);
        if (_sampleInput.Evaluate("ZoomOut") > 0.0f) camera.SetZoom(camera.Zoom * 0.99f);
    }

    protected override void Stop()
    {
        _sampleInput?.Dispose();
        _sampleInput = null;
        base.Stop();
    }
}
