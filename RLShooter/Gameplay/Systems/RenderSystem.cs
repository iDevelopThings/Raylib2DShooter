using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using DotTiled;
using RLShooter.App;
using RLShooter.Gameplay.Components;

namespace RLShooter.Gameplay.Systems;

public partial class RenderSystem : BaseSystem<World, float>, ISceneRenderSystem {
    public RenderSystem(World world) : base(world) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Update(in float data) {
        // RenderMap();

        RenderAllQuery(World, data);
        RenderAllSolidQuery(World, data);
    }
    /*private void RenderMap() {
        var map = Application.CurrentScene.Map;

        if (map == null)
            return;

        var layer = map.Layers.FirstOrDefault() as TileLayer;
        if (layer == null)
            return;

        var tileset = map.Tilesets.FirstOrDefault();
        if (tileset == null)
            return;

        
        
        var tileSize = new Vector2(tileset.TileWidth, tileset.TileHeight);
        var mapSize  = new Vector2(map.Width, map.Height);
        
        
        for (var y = 0; y < mapSize.Y; y++) {
            for (var x = 0; x < mapSize.X; x++) {
                // calculate the tile index from our x and y
                var tileIdx = (int)(x + y * mapSize.X);
                var tile    = layer.Data.Value.GlobalTileIDs.Value[tileIdx];
                
                if (tile == 0)
                    continue;
                
                tileset.
            }
        }

    }*/

    [Query]
    [All<SpriteRenderable, Position>]
    public void RenderAll([Data] float time, ref SpriteRenderable sprite, ref Position pos) {
        if (!IsTextureValid(sprite.Texture))
            return;

        var width  = sprite.Texture.Width * pos.Scale.X;
        var height = sprite.Texture.Height * pos.Scale.Y;


        var src = new Rectangle(0, 0, sprite.Texture.Width, sprite.Texture.Height);
        var dest = new Rectangle(
            pos.Global.X,
            pos.Global.Y,
            width,
            height
        );
        var origin = new Vector2(
            sprite.Pivot.X * width,
            sprite.Pivot.Y * height
        );

        DrawTexturePro(
            sprite.Texture,
            src,
            dest,
            origin,
            pos.GlobalRotation,
            sprite.Tint
        );
    }
    [Query]
    [All<SolidSpriteBoxRender, Position>]
    public void RenderAllSolid([Data] float time, ref SolidSpriteBoxRender sprite, ref Position pos) {

        var width  = sprite.Size.X * pos.Scale.X;
        var height = sprite.Size.Y * pos.Scale.Y;
        
        var src = new Rectangle(0, 0, sprite.Size.X, sprite.Size.Y);
        var dest = new Rectangle(
            pos.Global.X,
            pos.Global.Y,
            width,
            height
        );
        var origin = new Vector2(
            0,0
            // sprite.Pivot.X * width,
            // sprite.Pivot.Y * height
        );

        DrawRectanglePro(
            dest,
            origin,
            pos.GlobalRotation,
            sprite.Color
        );
    }
}