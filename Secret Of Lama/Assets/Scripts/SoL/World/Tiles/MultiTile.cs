using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Multi Tile", menuName = "Tiles/Multi")]
    public class MultiTile : TileBase
    {
        public Sprite sprite;
        public Vector2Int tileOffset;
        public Vector2Int collisionBoxSize = Vector2Int.one;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = sprite;
            tileData.colliderType = Tile.ColliderType.Grid;
        }


        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            if (go != null)
            {
                var box = go.AddComponent<BoxCollider2D>();
                box.size = collisionBoxSize;
            }
            return base.StartUp(position, tilemap, go);
        }

    }
}
