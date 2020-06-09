using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using UnityEngine;
using SoL.Audio;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Stair Tile", menuName = "Tiles/Stair")]
    public class StairTile : Tile
    {
        public int numSteps;
        public Vector3 upDirection;
        public float heightGainedPerStep;

        public float localStairStartMin = 0f;
        public float localStairStartMax = 1f;

        public Soundbank stepSound;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
        }
    }
}
