using SoL.Actors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoL.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Tiles
{
    [CreateAssetMenu(fileName = "Destructible Tile", menuName = "Tiles/Destructible")]
    public class DestructibleTile : AnimatedTile, ISerializableTile, ISerializationCallbackReceiver
    {
        public Tile destroyedTile;
        public Sprite[] aliveSprites;

        public ColliderType aliveColliderType;
        
        protected bool dead;

        public bool IsDead => dead;

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
            {
                //tilemap.SetTile(location, destroyedTile);
                var newTile = ScriptableObject.CreateInstance<DestructibleTile>();
                newTile.dead = true;
                newTile.sprite = destroyedTile.sprite;
                newTile.sprites = new[] {  destroyedTile.sprite };
                newTile.colliderType = ColliderType.None;
                newTile.destroyedTile = this.destroyedTile;
                newTile.aliveSprites = this.aliveSprites;
                newTile.aliveColliderType = this.aliveColliderType;
                tilemap.SetTile(location, newTile);
                


            }
        }

        public virtual void Revive(Vector3Int location, Tilemap tilemap)
        {
            var newTile = ScriptableObject.CreateInstance<DestructibleTile>();
            newTile.dead = false;
            newTile.sprite = this.aliveSprites[0];
            newTile.sprites = this.aliveSprites;
            newTile.colliderType = this.aliveColliderType;
            newTile.destroyedTile = this.destroyedTile;
            newTile.aliveSprites = this.aliveSprites;
            newTile.aliveColliderType = this.aliveColliderType;
            tilemap.SetTile(location, newTile);
        }

        public virtual void Serialize(BinaryWriter bw, Vector3Int location, Tilemap tilemap)
        {
            bw.Write(dead);
        }

        public virtual void Deserialize(BinaryReader br, Vector3Int location, Tilemap tilemap)
        {
            dead = br.ReadBoolean();
            if (dead)
            {
                Kill(location, tilemap);
            }
            else
            {
                Revive(location, tilemap);
            }
        }

        public void OnBeforeSerialize()
        {
            if (this.sprites == null || this.sprites.Length == 0)
                this.aliveSprites = new[] { this.sprite };
            else
                this.aliveSprites = this.sprites;
            
            this.aliveColliderType = this.colliderType;
        }

        public void OnAfterDeserialize()
        {
            
        }
    }
}
