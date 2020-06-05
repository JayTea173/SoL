using SoL.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Rounded Tile", menuName = "Tiles/Rounded")]
    public class RoundedTile : DestructibleTile
    {
        [Tooltip("If two different tiles have same sharedRoundingId, they will act as if they are connected.")]
        public int sharedRoundingId;
        public Sprite[] tl, t, tr, l, r, bl, b, br;

        private Sprite[] animatedTiles;
        public bool IsDestructible
        {
            get
            {
                return this.destroyedTile != null;
            }
        }

        protected bool DisconnectedFrom(Vector3Int position, ITilemap tilemap)
        {
            var tile = tilemap.GetTile(position);
            if (tile is RoundedTile)
            {
                var r = tile as RoundedTile;
                return r.sharedRoundingId != this.sharedRoundingId;
            }
            return true;
        }

        public override bool GetTileAnimationData(Vector3Int location, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            tileAnimationData.animatedSprites = animatedTiles;
            tileAnimationData.animationSpeed = fps;
            tileAnimationData.animationStartTime = 0f;
            return true;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            bool bl = DisconnectedFrom(position.MoveDownLeft(), tilemap);
            bool b = DisconnectedFrom(position.MoveDown(), tilemap);
            bool br = DisconnectedFrom(position.MoveDownRight(), tilemap);
            bool l = DisconnectedFrom(position.MoveLeft(), tilemap);
            bool r = DisconnectedFrom(position.MoveRight(), tilemap);
            bool tl = DisconnectedFrom(position.MoveUpLeft(), tilemap);
            bool t = DisconnectedFrom(position.MoveUp(), tilemap);
            bool tr = DisconnectedFrom(position.MoveUpRight(), tilemap);


            animatedTiles = sprites;
            if (b)
            {
                //bl, b, br
                if (l)
                {
                    if (bl && b && !r && !t && !tr)
                        animatedTiles = this.bl;
                } else
                {
                    if (r)
                        animatedTiles = this.br;
                    else if (!t)
                        animatedTiles = this.b;
                }
            } else
            {
                //l, r, tl, t, tr
                if (l)
                {
                    if (t)
                    {
                        if (!b && !r && tl)
                            animatedTiles = this.tl;
                    } else
                    {
                        if (!b && !r)
                            animatedTiles = this.l;
                    }
                } else if (!l)
                {
                    if (r)
                    {
                        if (t)
                        {
                            if (tr)
                                animatedTiles = this.tr;
                        } else
                            animatedTiles = this.r;

                    } else
                    {
                        if (t)
                            animatedTiles = this.t;
                    }
                    
                }
            }


            tileData.sprite = animatedTiles[0];
            



        }

    }
}
