// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Scene2D;

using RedBolt.Core;
using RedBolt.Graphics;
using RedBolt.Math;
using RedBolt.Resource;
using RedBolt.Scene;

/// <summary>Runtime component for one TMX layer.</summary>
[Reflected]
public partial class TileMapLayer2D : Component
{
    private const int IsometricBackgroundLayer = short.MinValue;

    private TileMap2D? _tileMap;
    private TmxLayer2D? _tmxLayer;
    private TmxTileLayer2D? _tileLayer;
    private TmxObjectGroup2D? _objectGroup;
    private TmxImageLayer2D? _imageLayer;
    private int _drawOrder;
    private bool _visible = true;
    private readonly List<Node?> _nodes = [];

    public TileMap2D? TileMap => _tileMap;
    public TmxLayer2D? TmxLayer => _tmxLayer;
    public int DrawOrder
    {
        get => _drawOrder;
        set
        {
            if (_drawOrder == value)
                return;
            _drawOrder = value;
            if (IsIsometricDepthSorted)
            {
                foreach (var node in _nodes)
                    if (node?.GetComponent<StaticSprite2D>() is { } sprite)
                        sprite.OrderInLayer = value;
            }
            if (_tileMap?.Info.Orientation == Orientation2D.Isometric)
                return;
            foreach (var node in _nodes)
                if (node?.GetComponent<StaticSprite2D>() is { } sprite)
                    ApplyDrawOrder(sprite, node.GetPosition2D());
        }
    }
    public bool IsVisible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;
            _visible = value;
            foreach (var node in _nodes)
                if (node is not null) node.Enabled = value;
        }
    }
    public TileMapLayerType2D LayerType => _tmxLayer?.LayerType ?? TileMapLayerType2D.Invalid;
    public int Width => _tmxLayer?.Width ?? 0;
    public int Height => _tmxLayer?.Height ?? 0;
    public uint NumObjects => _objectGroup?.NumObjects ?? 0;
    public bool HasProperty(string name) => _tmxLayer?.HasProperty(name) == true;
    public string GetProperty(string name) => _tmxLayer?.GetProperty(name) ?? string.Empty;
    public Tile2D? GetTile(int x, int y) => _tileLayer?.GetTile(x, y);
    public TileMapObject2D? GetObject(uint index) => _objectGroup?.GetObject(index);
    public Node? GetImageNode() => _imageLayer is not null && _nodes.Count != 0 ? _nodes[0] : null;
    public Node? GetObjectNode(uint index) => _objectGroup is not null && index < _nodes.Count ? _nodes[(int)index] : null;
    public Node? GetTileNode(int x, int y) => _tileLayer is null || x < 0 || x >= Width || y < 0 || y >= Height
        ? null : _nodes[y * Width + x];

    public void Initialize(TileMap2D tileMap, TmxLayer2D tmxLayer)
    {
        ArgumentNullException.ThrowIfNull(tileMap);
        ArgumentNullException.ThrowIfNull(tmxLayer);
        if (ReferenceEquals(_tileMap, tileMap) && ReferenceEquals(_tmxLayer, tmxLayer))
            return;
        foreach (var node in _nodes)
            node?.Remove();
        _nodes.Clear();
        _tileLayer = null;
        _objectGroup = null;
        _imageLayer = null;
        _tileMap = tileMap;
        _tmxLayer = tmxLayer;
        switch (tmxLayer)
        {
            case TmxTileLayer2D tileLayer: SetTileLayer(tileLayer); break;
            case TmxObjectGroup2D objectGroup: SetObjectGroup(objectGroup); break;
            case TmxImageLayer2D imageLayer: SetImageLayer(imageLayer); break;
        }
        IsVisible = tmxLayer.IsVisible;
        foreach (var node in _nodes)
            if (node is not null) node.Enabled = _visible;
    }

    public override void DrawDebugGeometry(DebugRenderer? debug, bool depthTest)
    {
        if (debug is null || _objectGroup is null || _tileMap?.Node is not { } mapNode)
            return;
        var transform = mapNode.GetWorldTransform();
        for (uint index = 0; index < _objectGroup.NumObjects; ++index)
        {
            var value = _objectGroup.GetObject(index);
            if (value is null)
                continue;
            if (value.ObjectType is TileMapObjectType2D.Polygon or TileMapObjectType2D.Polyline)
            {
                for (uint point = 1; point < value.NumPoints; ++point)
                    AddLine(debug, transform, value.GetPoint(point - 1), value.GetPoint(point), depthTest);
                if (value.ObjectType == TileMapObjectType2D.Polygon && value.NumPoints > 2)
                    AddLine(debug, transform, value.GetPoint(value.NumPoints - 1), value.GetPoint(0), depthTest);
            }
            else
            {
                var position = value.Position;
                var size = value.Size;
                AddLine(debug, transform, position, position + new Vector2f(size.X, 0), depthTest);
                AddLine(debug, transform, position + new Vector2f(size.X, 0), position + size, depthTest);
                AddLine(debug, transform, position + size, position + new Vector2f(0, size.Y), depthTest);
                AddLine(debug, transform, position + new Vector2f(0, size.Y), position, depthTest);
            }
        }
    }

    private void SetTileLayer(TmxTileLayer2D layer)
    {
        _tileLayer = layer;
        for (var index = 0; index < layer.Width * layer.Height; ++index)
            _nodes.Add(null);
        var info = _tileMap!.Info;
        for (var y = 0; y < layer.Height; ++y)
        for (var x = 0; x < layer.Width; ++x)
        {
            var tile = layer.GetTile(x, y);
            if (tile is null)
                continue;
            var node = Node.CreateTemporaryChild("Tile");
            node.SetPosition2D(info.TileIndexToPosition(x, y) + layer.Offset);
            var sprite = node.CreateComponent<StaticSprite2D>();
            sprite.Sprite = tile.Sprite;
            sprite.SetFlip(tile.FlipX, tile.FlipY, tile.SwapXY);
            sprite.OrderInLayer = y * layer.Width + x;
            var groundPosition = node.GetPosition2D();
            if (info.Orientation == Orientation2D.Isometric)
                groundPosition += new Vector2f(info.TileWidth * 0.5f, info.TileHeight * 0.5f);
            ApplyDrawOrder(sprite, groundPosition);
            _nodes[y * layer.Width + x] = node;
        }
    }

    private void SetObjectGroup(TmxObjectGroup2D layer)
    {
        _objectGroup = layer;
        for (uint index = 0; index < layer.NumObjects; ++index)
        {
            var value = layer.GetObject(index)!;
            var node = Node.CreateTemporaryChild("Object");
            node.SetPosition2D(value.Position);
            if (value.ObjectType == TileMapObjectType2D.Tile && value.TileGid != 0 && value.TileSprite is not null)
            {
                var sprite = node.CreateComponent<StaticSprite2D>();
                sprite.Sprite = value.TileSprite;
                sprite.SetFlip(value.TileFlipX, value.TileFlipY, value.TileSwapXY);
                sprite.OrderInLayer = (int)((10.0f - value.Position.Y) * 100.0f);
                ApplyDrawOrder(sprite, value.Position);
                if (_tileMap!.Info.Orientation == Orientation2D.Isometric)
                {
                    sprite.UseHotSpot = true;
                    sprite.HotSpot = new Vector2f(0.5f, 0.0f);
                }
            }
            _nodes.Add(node);
        }
    }

    private void SetImageLayer(TmxImageLayer2D layer)
    {
        _imageLayer = layer;
        if (layer.Sprite is null)
            return;
        var node = Node.CreateTemporaryChild("Image");
        node.SetPosition2D(layer.Position);
        var sprite = node.CreateComponent<StaticSprite2D>();
        sprite.Sprite = layer.Sprite;
        sprite.Layer = _drawOrder;
        sprite.OrderInLayer = 0;
        _nodes.Add(node);
    }

    private static void AddLine(DebugRenderer debug, Matrix3x4 transform, Vector2f start, Vector2f end, bool depthTest) =>
        debug.AddLine(transform * new Vector3f(start.X, start.Y, 0.0f),
            transform * new Vector3f(end.X, end.Y, 0.0f), Color.Yellow, depthTest);

    private void ApplyDrawOrder(StaticSprite2D sprite, Vector2f groundPosition)
    {
        if (IsIsometricDepthSorted)
        {
            sprite.Layer = -(int)(groundPosition.Y * 100.0f);
            sprite.OrderInLayer = _drawOrder;
        }
        else if (_tileMap!.Info.Orientation == Orientation2D.Isometric)
            sprite.Layer = IsometricBackgroundLayer;
        else
            sprite.Layer = _drawOrder;
    }

    private bool IsIsometricDepthSorted => _tileMap?.Info.Orientation == Orientation2D.Isometric &&
        !string.Equals(_tmxLayer?.GetProperty("DepthSorted"), "false", StringComparison.OrdinalIgnoreCase);
}
