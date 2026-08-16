// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Web;

using RedBolt.Engine;

internal static class SampleCatalog
{
    internal static Application Create(string applicationTypeName) => applicationTypeName switch
    {
        "RedBolt.Sample.Urho2DSprite.Urho2DSprite" => new global::RedBolt.Sample.Urho2DSprite.Urho2DSprite(),
        "RedBolt.Sample.Urho2DParticle.Urho2DParticle" => new global::RedBolt.Sample.Urho2DParticle.Urho2DParticle(),
        "RedBolt.Sample.Urho2DSpriterAnimation.Urho2DSpriterAnimation" => new global::RedBolt.Sample.Urho2DSpriterAnimation.Urho2DSpriterAnimation(),
        "RedBolt.Sample.Urho2DTileMap.Urho2DTileMap" => new global::RedBolt.Sample.Urho2DTileMap.Urho2DTileMap(),
        "RedBolt.Sample.Urho2DStretchableSprite.Urho2DStretchableSprite" => new global::RedBolt.Sample.Urho2DStretchableSprite.Urho2DStretchableSprite(),
        _ => throw new System.InvalidOperationException($"Unknown RedBolt sample application: {applicationTypeName}"),
    };
}
