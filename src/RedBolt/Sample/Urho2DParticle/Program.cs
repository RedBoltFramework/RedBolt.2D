// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Urho2DParticle;

using System;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Graphics2D;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.Resource;
using RedBolt.Scene;
using RedBolt.Scene2D;

internal static class Urho2DParticleProgram
{
    private static int Main()
    {
        try { using var application = new Urho2DParticle(); return application.Run(); }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}

/// <summary>Demonstrates PEX particle effects in a 2D scene.</summary>
public sealed class Urho2DParticle : Application
{
    private Node? _particleNode;
    private Camera? _camera;

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Urho2DParticle";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.2D;Data.2D;CoreData;Data";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
        parameters.WindowHeight = 720;
        parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Urho2D Particle";
    }

    protected override void Start()
    {
        Scene2D.RegisterUrho2DLibrary(Context);
        var scene = new Scene();
        scene.CreateComponent<Octree>();
        var cameraNode = scene.CreateChild("Camera");
        cameraNode.SetPosition(new Vector3f(0.0f, 0.0f, -10.0f));
        _camera = cameraNode.CreateComponent<Camera>();
        _camera.SetOrthographic(true);
        _camera.OrthoSize = Graphics.Height * Sprite2D.PixelSize;
        _camera.SetZoom(1.2f * MathF.Min(Graphics.Width / 1280.0f, Graphics.Height / 800.0f));

        var blackSmoke = Resources.Get<ParticleEffect2D>("Urho2D/Smoke/blackSmoke.pex");
        var whitePuff = Resources.Get<ParticleEffect2D>("Urho2D/Smoke/whitePuff.pex");
        if (blackSmoke is not null)
        {
            _particleNode = CreateEmitter(scene, "CursorSmoke", blackSmoke, Vector2f.Zero, 0.70f);
            CreateEmitter(scene, "BlackSmokeLeft", blackSmoke, new Vector2f(-3.5f, 1.4f), 0.60f);
            CreateEmitter(scene, "BlackSmokeRight", blackSmoke, new Vector2f(3.5f, -1.3f), 0.60f);
        }
        if (whitePuff is not null)
        {
            CreateEmitter(scene, "WhitePuffLeft", whitePuff, new Vector2f(-3.0f, -1.7f), 0.60f);
            CreateEmitter(scene, "WhitePuffRight", whitePuff, new Vector2f(3.0f, 1.8f), 0.60f);
        }

        Renderer.SetViewport(0, new Viewport(scene, _camera));
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || Input.GetKeyPress(Key.Escape)) { Exit(); return; }
        if (_particleNode is null || _camera is null || Graphics.Width == 0 || Graphics.Height == 0) return;
        var mouse = Input.GetMousePosition();
        var world = _camera.ScreenToWorldPoint(new Vector3f(mouse.X / (float)Graphics.Width,
            mouse.Y / (float)Graphics.Height, 10.0f));
        _particleNode.SetPosition(world);
    }

    private static Node CreateEmitter(Scene scene, string name, ParticleEffect2D effect, Vector2f position, float scale)
    {
        var node = scene.CreateChild(name);
        node.SetPosition2D(position);
        node.SetScale(scale);
        var emitter = node.CreateComponent<ParticleEmitter2D>();
        emitter.Effect = effect;
        emitter.Layer = 1;
        return node;
    }
}
