using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Destructible Tile", menuName = "Tiles/Destructible")]
    public class DestructibleTile : AnimatedTile
    {
        public Tile destroyedTile;
        protected bool dead;

        public override bool GetTileAnimationData(Vector3Int location, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            bool b;
            if (dead)
                b = destroyedTile.GetTileAnimationData(location, tilemap, ref tileAnimationData);
            else
                b = base.GetTileAnimationData(location, tilemap, ref tileAnimationData);
            return b;
        }

        public void Kill(Vector3Int location, Tilemap tilemap)
        {
            if (destroyedTile != null)
                tilemap.SetTile(location, destroyedTile);
        }
    }
}
