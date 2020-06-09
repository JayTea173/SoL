using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Pathing
{
    [Serializable]
    public class PathingGrid : MonoBehaviour
    {
        [Serializable]
        public class Chunk : ISerializationCallbackReceiver
        {
            [SerializeField]
            private byte[] data;

            public bool Empty
            {
                get
                {
                    return data == null;
                }
            }

            public void Allocate()
            {
                data = new byte[chunkSizeX * chunkSizeY / 8];
            }

            public Chunk(bool empty = true)
            {
                if (!empty)
                    Allocate();
            }

            public bool this[int localX, int localY]
            {
                get
                {
                   
                    int arrayIndex;
                    int bitIndex;
                    GetLocalIndeces(localX, localY, out arrayIndex, out bitIndex);
                    
                    return ((data[arrayIndex] & (1 << bitIndex)) != 0);
                }

                set
                {
                    int arrayIndex;
                    int bitIndex;
                    GetLocalIndeces(localX, localY, out arrayIndex, out bitIndex);
                    if (value)
                    {
                        if ((data[arrayIndex] & (1 << bitIndex)) == 0)
                            data[arrayIndex] += (byte)(1 << bitIndex);
                    } else
                    {
                        if ((data[arrayIndex] & (1 << bitIndex)) != 0)
                            data[arrayIndex] -= (byte)(1 << bitIndex);
                    }
                }
            }

            private void GetLocalIndeces(int localX, int localY, out int arrayIndex, out int bitIndex) {
                int cellIndex = (localY * chunkSizeX + localX);
                arrayIndex = cellIndex / 8;
                bitIndex = cellIndex % 8;
            }

            public void OnBeforeSerialize()
            {
                
            }

            public void OnAfterDeserialize()
            {
                if (data != null)
                    if (data.Length == 0)
                        data = null;
            }
        }

        [SerializeField]
        protected Chunk[] chunks;

        public Vector2Int GetChunkContaining(Vector2Int world)
        {
            return new Vector2Int((world.x - xOrigin) / chunkSizeX, (world.y - yOrigin) / chunkSizeY);
        }
        
        public bool this[int worldX, int worldY]
        {
            get
            {
                if (InBounds(worldX, worldY))
                {
                    int cx = (worldX - xOrigin) / chunkSizeX;
                    int cy = (worldY - yOrigin) / chunkSizeY;
                    int localX = (worldX - xOrigin) % chunkSizeX;
                    int localY = (worldY - yOrigin) % chunkSizeY;
                    var chunk = chunks[cy * chunksX + cx];
                    if (chunk.Empty)
                        return false;
                    return chunk[localX, localY];
                }
                else
                    throw new Exception(worldX + ", " + worldY + " is outside of the pathing grid!");
            }
            set
            {
                if (InBounds(worldX, worldY))
                {
                    int cx = (worldX - xOrigin) / chunkSizeX;
                    int cy = (worldY - yOrigin) / chunkSizeY;
                    int localX = (worldX - xOrigin) % chunkSizeX;
                    int localY = (worldY - yOrigin) % chunkSizeY;
                    Debug.Log("ChunkId: " + cy * chunksX + cx + " getting " + worldX + ", " + worldY);
                    if (localX < 0 || localY < 0 || localX >= chunkSizeX || localY >= chunkSizeY)
                        Debug.LogError("Invalid local coordinates: " + localX + ", " + localY);
                    var chunk = chunks[cy * chunksX + cx];
                    
                    if (chunk.Empty)
                    {
                        if (value)
                            chunk.Allocate();
                        else
                            return;
                    }
                    chunk[localX, localY] = value;
                }
            }
        }

        public bool InBounds(int worldX, int worldY)
        {
            if (worldX < xOrigin || worldY < yOrigin)
                return false;

            if (worldX >= xOrigin + chunksX * chunkSizeX || worldY >= yOrigin + chunksY * chunkSizeY)
                return false;

            return true;
        }

        private void Awake()
        {
            Pathfinder.grid = this;
        }



        [SerializeField]
        protected int chunksX;
        [SerializeField]
        protected int chunksY;
        [SerializeField]
        protected int xOrigin;
        [SerializeField]
        protected int yOrigin;

        public Vector2Int origin
        {
            get
            {
                return new Vector2Int(xOrigin, yOrigin);
            }
        }

        public Vector2Int size
        {
            get
            {
                return new Vector2Int(chunksX * chunkSizeX, chunksY * chunkSizeY);
            }
        }

        public static readonly int chunkSizeX = 16;
        public static readonly int chunkSizeY = 16;

        public void Resize(int w, int h)
        {
            chunksX = Mathf.CeilToInt(w / (float)chunkSizeX);
            chunksY = Mathf.CeilToInt(h / (float)chunkSizeY);
            chunks = new Chunk[chunksX * chunksY];
        }

        public void SetOrigin(int x, int y)
        {
            xOrigin = x;
            yOrigin = y;
        }

        private Tilemap tm;

#if UNITY_EDITOR
        public bool drawGizmos;
#endif

        public Mesh gizmosQuadMesh;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            if (drawGizmos)
            {

               
                
                for (int cy = 0; cy < chunksY; cy++)
                {
                    for (int cx = 0; cx < chunksX; cx++)
                    {
                        int yMin = yOrigin + cy * chunkSizeY;
                        int yMax = yMin + chunkSizeY;
                        int xMin = xOrigin + cx * chunkSizeX;
                        int xMax = xMin + chunkSizeX;

                        if (chunks[cy * chunksY + cx].Empty)
                        {
                            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                            Gizmos.DrawMesh(gizmosQuadMesh, 0, new Vector3(xMin + chunkSizeX / 2, yMin + chunkSizeY / 2, 5f), Quaternion.identity, new Vector3(chunkSizeX, chunkSizeY));
                        }
                        else
                        {
                            


                            for (int y = yMin; y < yMax; y++)
                            {
                                for (int x = xMin; x < xMax; x++)
                                {
                                    if (this[x, y])
                                        Gizmos.color = new Color(1f, 0f, 0f, 0.66f);
                                    else
                                        Gizmos.color = new Color(0f, 1f, 0f, 0.33f);
                                    Gizmos.DrawMesh(gizmosQuadMesh, 0, new Vector3(x + 0.5f, y + 0.5f, 5f));
                                }
                            }
                        }
                        //Gizmos.DrawCube(new Vector3(x, y, 0f), Vector3.one);
                        
                    }
                }
                
            }
        }
#endif

    }
}
