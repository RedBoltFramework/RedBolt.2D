// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Urho2DSpriterAnimation;

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

internal static class Urho2DSpriterAnimationProgram
{
    private static int Main()
    {
        try { using var application = new Urho2DSpriterAnimation(); return application.Run(); }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}

/// <summary>Demonstrates SCML Spriter animation playback.</summary>
public sealed class Urho2DSpriterAnimation : Application
{
    private SampleInputMap? _sampleInput;
    private Node? _cameraNode;
    private AnimatedSprite2D? _animatedSprite;
    private AnimationSet2D? _animationSet;
    private Texture2D[]? _controlPrompts;
    private uint _animationIndex;
    private static readonly Vector2f PromptIconSize = new(24.0f, 24.0f);

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Urho2DSpriterAnimation";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.2D;SampleData.2D;CoreData;SampleData";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
        parameters.WindowHeight = 720;
        parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Urho2D Spriter Animation";
    }

    protected override void Start()
    {
        _sampleInput = new SampleInputMap(movement: true);
        _sampleInput.AddButton("ZoomIn", Scancode.PageUp, ControllerButton.RightShoulder, "+", 1);
        _sampleInput.AddButton("ZoomOut", Scancode.PageDown, ControllerButton.LeftShoulder, "-", 2);
        _sampleInput.AddMouseButton("NextAnimation", MouseButton.Left, ControllerButton.A, "NEXT");
        _sampleInput.Map.MapKeyboardKey("NextAnimation", Scancode.Space);
        Scene2D.RegisterUrho2DLibrary(Context);
        var scene = new Scene();
        scene.CreateComponent<Octree>();
        _cameraNode = scene.CreateChild("Camera");
        _cameraNode.SetPosition(new Vector3f(0.0f, 0.0f, -10.0f));
        var camera = _cameraNode.CreateComponent<Camera>();
        camera.SetOrthographic(true);
        camera.OrthoSize = Graphics.Height * Sprite2D.PixelSize;
        camera.SetZoom(1.5f * MathF.Min(Graphics.Width / 1280.0f, Graphics.Height / 800.0f));

        _animationSet = Resources.Get<AnimationSet2D>("Urho2D/imp/imp.scml");
        if (_animationSet is not null && _animationSet.NumAnimations != 0)
        {
            _animatedSprite = scene.CreateChild("SpriterAnimation").CreateComponent<AnimatedSprite2D>();
            _animatedSprite.AnimationSet = _animationSet;
            _animatedSprite.SetAnimation(_animationSet.GetAnimation(0));
        }
        CreateInstructions();
        Renderer.SetViewport(0, new Viewport(scene, camera));
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || _sampleInput!.Pressed("Exit")) { Exit(); return; }
        const float speed = 4.0f;
        var movement = Vector2f.Zero;
        movement.Y += (_sampleInput.Evaluate("Forward") - _sampleInput.Evaluate("Back")) * speed * timeStep;
        movement.X += (_sampleInput.Evaluate("Right") - _sampleInput.Evaluate("Left")) * speed * timeStep;
        _cameraNode!.Translate2D(movement);
        var camera = _cameraNode.GetComponent<Camera>()!;
        if (_sampleInput.Evaluate("ZoomIn") > 0.0f) camera.SetZoom(camera.Zoom * 1.01f);
        if (_sampleInput.Evaluate("ZoomOut") > 0.0f) camera.SetZoom(camera.Zoom * 0.99f);
        if (_sampleInput.Pressed("NextAnimation") &&
            _animationSet is { NumAnimations: > 0 } && _animatedSprite is not null)
        {
            _animationIndex = (_animationIndex + 1) % _animationSet.NumAnimations;
            _animatedSprite.SetAnimation(_animationSet.GetAnimation(_animationIndex), LoopMode2D.ForceLooped);
        }
    }

    protected override void Stop()
    {
        _sampleInput?.Dispose();
        _sampleInput = null;
        base.Stop();
    }

    private void CreateInstructions()
    {
        Context.RegisterSubsystem(new DearImGui((ImGuiConfigFlags)Engine.Parameters.DearImGuiFlags));
        SubscribeToEvent<DearImGuiEvents.EndRenderingDearImGuiEvent>(
            (ref DearImGuiEvents.EndRenderingDearImGuiEvent _) => DrawInstructions());
        _controlPrompts =
        [
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_w.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_a.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_s.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_d.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_arrow_up.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_arrow_down.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/mouse_left.png"),
            Resources.Get<Texture2D>("ThirdParty/KenneyInputPrompts/keyboard_space.png")
        ];
    }

    private void DrawInstructions()
    {
        var windowOpen = true;
        ImGui.SetNextWindowPos(new Vector2f(10.0f, 10.0f), ImGuiCond.ImGuiCondAlways);
        if (ImGui.Begin("Spriter Animation"u8, ref windowOpen,
                ImGuiWindowFlags.ImGuiWindowFlagsNoResize | ImGuiWindowFlags.ImGuiWindowFlagsNoSavedSettings))
        {
            for (var i = 0; i < 3; ++i)
            {
                ImGui.Image(_controlPrompts![i], PromptIconSize);
                ImGui.SameLine();
            }
            DrawPrompt(_controlPrompts![3], "Move camera"u8);
            ImGui.Image(_controlPrompts[4], PromptIconSize);
            ImGui.SameLine();
            DrawPrompt(_controlPrompts[5], "Zoom"u8);
            ImGui.Image(_controlPrompts[6], PromptIconSize);
            ImGui.SameLine();
            DrawPrompt(_controlPrompts[7], "Next animation"u8);
        }
        ImGui.End();
    }

    private static void DrawPrompt(Texture2D texture, ReadOnlySpan<byte> label)
    {
        ImGui.Image(texture, PromptIconSize);
        ImGui.SameLine();
        Widgets.AlignTextToLastItem();
        ImGui.TextUnformatted(label);
    }
}
