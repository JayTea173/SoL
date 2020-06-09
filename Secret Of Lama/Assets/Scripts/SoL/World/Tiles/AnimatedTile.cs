using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{

    [CreateAssetMenu(fileName = "Animated Tile", menuName = "Tiles/Animated")]
    public class AnimatedTile : Tile
    {
        public Sprite[] sprites;
        public float fps = 8f;
        protected int currentIndex;

        public override bool GetTileAnimationData(Vector3Int location, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            if (sprites == null)
                return false;
            if (sprites.Length == 0)
                return false;
            tileAnimationData.animatedSprites = sprites;
            tileAnimationData.animationSpeed = fps;
            tileAnimationData.animationStartTime = 0f;
            return true;
        }


    }
}
