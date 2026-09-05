// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Urho2DParticle;

using System;
using RedBolt;
using RedBolt.Bindings.DearImGui;
using RedBolt.Core;
using RedBolt.DearImGui;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Graphics2D;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.Resource;
using RedBolt.Scene;
using RedBolt.Scene2D;
using Widgets = RedBolt.SystemUI.Widgets;

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
    private SampleInputMap? _sampleInput;
    private Node? _particleNode;
    private readonly Node?[] _ribbonEmitters = new Node?[3];
    private Camera? _camera;
    private Texture2D? _pointerPrompt;
    private float _sceneTime;
    private static readonly Vector2f PromptIconSize = new(24.0f, 24.0f);

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Urho2DParticle";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.2D;SampleData.2D;CoreData;SampleData";
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
        _sampleInput = new SampleInputMap();
        _sampleInput.Map.MapControllerAxis("GuideLeft", ControllerAxis.LeftX, 0.0f, -1.0f);
        _sampleInput.Map.MapControllerAxis("GuideRight", ControllerAxis.LeftX, 0.0f, 1.0f);
        _sampleInput.Map.MapControllerAxis("GuideUp", ControllerAxis.LeftY, 0.0f, -1.0f);
        _sampleInput.Map.MapControllerAxis("GuideDown", ControllerAxis.LeftY, 0.0f, 1.0f);
        Scene2D.RegisterUrho2DLibrary(Context);
        var scene = new Scene();
        scene.CreateComponent<Octree>();
        var zone = scene.CreateChild("NightSky").CreateComponent<Zone>();
        zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
        zone.SetFogColor(new Color(0.015f, 0.025f, 0.075f));
        var cameraNode = scene.CreateChild("Camera");
        cameraNode.SetPosition(new Vector3f(0.0f, 0.0f, -10.0f));
        _camera = cameraNode.CreateComponent<Camera>();
        _camera.SetOrthographic(true);
        _camera.OrthoSize = Graphics.Height * Sprite2D.PixelSize;
        _camera.SetZoom(1.2f * MathF.Min(Graphics.Width / 1280.0f, Graphics.Height / 800.0f));

        var whitePuff = Resources.Get<ParticleEffect2D>("Urho2D/Smoke/whitePuff.pex");
        if (whitePuff is not null)
        {
            _ribbonEmitters[0] = CreateEmitter(scene, "RoseLight",
                CreateLightEffect(whitePuff, "Rose", new Color(1.0f, 0.12f, 0.36f, 0.85f)), Vector2f.Zero, 1.0f);
            _ribbonEmitters[1] = CreateEmitter(scene, "VioletLight",
                CreateLightEffect(whitePuff, "Violet", new Color(0.65f, 0.18f, 1.0f, 0.72f)), Vector2f.Zero, 0.85f);
            _ribbonEmitters[2] = CreateEmitter(scene, "BlueLight",
                CreateLightEffect(whitePuff, "Blue", new Color(0.12f, 0.65f, 1.0f, 0.65f)), Vector2f.Zero, 0.7f);

            var pointerEffect = CreateLightEffect(whitePuff, "Pointer", new Color(1.0f, 0.72f, 0.18f, 0.9f));
            pointerEffect.MaxParticles = 90;
            pointerEffect.ParticleLifeSpan = 1.1f;
            pointerEffect.ParticleLifespanVariance = 0.25f;
            pointerEffect.StartParticleSize = 24.0f;
            pointerEffect.FinishParticleSize = 58.0f;
            _particleNode = CreateEmitter(scene, "GuidingLight", pointerEffect, Vector2f.Zero, 0.75f);
        }

        CreateInstructions();
        Renderer.SetViewport(0, new Viewport(scene, _camera));
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || _sampleInput!.Pressed("Exit")) { Exit(); return; }
        _sceneTime += timeStep;
        for (var i = 0; i < _ribbonEmitters.Length; ++i)
        {
            var phase = _sceneTime * 1.05f - i * 0.22f;
            _ribbonEmitters[i]?.SetPosition2D(GetFlowPosition(phase, i));
        }

        if (_particleNode is not null && _camera is not null && Graphics.Width != 0 && Graphics.Height != 0)
        {
            if (Input.ActiveInputDevice == InputDeviceType.Joystick)
            {
                var position = _particleNode.GetPosition2D();
                position += new Vector2f(
                    _sampleInput.Evaluate("GuideRight") - _sampleInput.Evaluate("GuideLeft"),
                    _sampleInput.Evaluate("GuideUp") - _sampleInput.Evaluate("GuideDown")) * (4.0f * timeStep);
                _particleNode.SetPosition2D(position);
            }
            else
            {
                var pointer = Input.GetMousePosition();
                var world = _camera.ScreenToWorldPoint(new Vector3f(pointer.X / (float)Graphics.Width,
                    pointer.Y / (float)Graphics.Height, 10.0f));
                _particleNode.SetPosition(world);
            }
        }
    }

    protected override void Stop()
    {
        _sampleInput?.Dispose();
        _sampleInput = null;
        base.Stop();
    }

    private static ParticleEffect2D CreateLightEffect(ParticleEffect2D source, string name, Color color)
    {
        var effect = source.Clone(name);
        effect.SourcePositionVariance = new Vector2f(2.0f, 2.0f);
        effect.Speed = 4.0f;
        effect.SpeedVariance = 3.0f;
        effect.Gravity = Vector2f.Zero;
        effect.AngleVariance = 360.0f;
        effect.ParticleLifeSpan = 5.4f;
        effect.ParticleLifespanVariance = 0.3f;
        effect.MaxParticles = 260;
        effect.StartParticleSize = 32.0f;
        effect.StartParticleSizeVariance = 10.0f;
        effect.FinishParticleSize = 86.0f;
        effect.FinishParticleSizeVariance = 22.0f;
        effect.StartColor = color;
        effect.StartColorVariance = new Color(0.08f, 0.08f, 0.08f, 0.08f);
        effect.FinishColor = new Color(color.R * 0.45f, color.G * 0.45f, color.B * 0.55f, 0.0f);
        effect.FinishColorVariance = new Color(0.04f, 0.04f, 0.04f, 0.0f);
        effect.BlendMode = BlendMode.AddAlpha;
        return effect;
    }

    private static Vector2f GetFlowPosition(float phase, int ribbon)
    {
        var offset = ribbon * 0.42f;
        var x = 4.8f * MathF.Sin(phase) + 0.35f * MathF.Sin(3.0f * phase + offset);
        var y = 1.65f * MathF.Sin(2.0f * phase + offset) +
            0.32f * MathF.Cos(3.0f * phase - offset);
        return new Vector2f(x, y);
    }

    private void CreateInstructions()
    {
        Context.RegisterSubsystem(new DearImGui((ImGuiConfigFlags)Engine.Parameters.DearImGuiFlags));
        SubscribeToEvent<DearImGuiEvents.EndRenderingDearImGuiEvent>(
            (ref DearImGuiEvents.EndRenderingDearImGuiEvent _) => DrawInstructions());
        _pointerPrompt = Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/mouse_move.png");
    }

    private void DrawInstructions()
    {
        var windowOpen = true;
        ImGui.SetNextWindowPos(new Vector2f(10.0f, 10.0f), ImGuiCond.ImGuiCondAlways);
        if (ImGui.Begin("Particle"u8, ref windowOpen,
                ImGuiWindowFlags.ImGuiWindowFlagsNoResize | ImGuiWindowFlags.ImGuiWindowFlagsNoSavedSettings))
        {
            ImGui.TextUnformatted("Layered 2D particle trails in motion"u8);
            if (_pointerPrompt is not null)
            {
                ImGui.Image(Input.ActiveInputDevice switch
                {
                    InputDeviceType.Touch => Resources.Get<Texture2D>(
                        "ThirdParty/KenneyInputPrompts/touch_swipe_move.png"),
                    InputDeviceType.Joystick => SampleInputMap.GetControllerPrompt(ControllerButton.LeftStick),
                    _ => _pointerPrompt
                }, PromptIconSize);
                ImGui.SameLine();
                Widgets.AlignTextToLastItem();
                ImGui.TextUnformatted("Guide the golden light"u8);
            }
        }
        ImGui.End();
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
