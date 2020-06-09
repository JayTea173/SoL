using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Liquid Tile", menuName = "Tiles/Data/Liquid")]
    public class LiquidTile : TileBase
    {
#if UNITY_EDITOR
        public Sprite paletteSprite;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            if (!UnityEditor.EditorApplication.isPlaying)
                tileData.sprite = paletteSprite;
        }
    }
#endif
}
