namespace RedBolt.Sample.Android;

internal static class SampleCatalog
{
    internal static readonly SampleEntry[] Entries =
    [
        new("urho2d-sprite", "Urho2D Sprite", static () => new global::RedBolt.Sample.Urho2DSprite.Urho2DSprite()),
        new("urho2d-particle", "Urho2D Particle", static () => new global::RedBolt.Sample.Urho2DParticle.Urho2DParticle()),
        new("urho2d-spriter-animation", "Urho2D Spriter Animation", static () => new global::RedBolt.Sample.Urho2DSpriterAnimation.Urho2DSpriterAnimation()),
        new("urho2d-tile-map", "Urho2D Tile Map", static () => new global::RedBolt.Sample.Urho2DTileMap.Urho2DTileMap()),
        new("urho2d-stretchable-sprite", "Urho2D Stretchable Sprite", static () => new global::RedBolt.Sample.Urho2DStretchableSprite.Urho2DStretchableSprite()),
    ];
}
