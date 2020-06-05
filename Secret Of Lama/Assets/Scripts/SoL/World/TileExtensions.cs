using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL
{
    public static class TileExtensions
    {
        public static Vector3Int MoveRight(this Vector3Int v)
        {
            return new Vector3Int(v.x + 1, v.y, v.z);
        }
        public static Vector3Int MoveLeft(this Vector3Int v)
        {
            return new Vector3Int(v.x - 1, v.y, v.z);
        }
        public static Vector3Int MoveUp(this Vector3Int v)
        {
            return new Vector3Int(v.x, v.y + 1, v.z);
        }
        public static Vector3Int MoveDown(this Vector3Int v)
        {
            return new Vector3Int(v.x, v.y - 1, v.z);
        }
        public static Vector3Int MoveUpRight(this Vector3Int v)
        {
            return new Vector3Int(v.x + 1, v.y + 1, v.z);
        }
        public static Vector3Int MoveUpLeft(this Vector3Int v)
        {
            return new Vector3Int(v.x - 1, v.y + 1, v.z);
        }
        public static Vector3Int MoveDownRight(this Vector3Int v)
        {
            return new Vector3Int(v.x + 1, v.y - 1, v.z);
        }
        public static Vector3Int MoveDownLeft(this Vector3Int v)
        {
            return new Vector3Int(v.x - 1, v.y - 1, v.z);
        }
    }
}
