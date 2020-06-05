﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL
{
    public class World : MonoBehaviour
    {
        public Tilemap[] tilemapLayers;

        private static World instance;
        public static World Instance
        {
            get
            {
                return instance;
            }
        }
        

        private void Awake()
        {
            instance = this;
        }

        public IEnumerable<Vector3Int> GetTilesInRadius(int layer, Vector3 point, float radius)
        {
            int iRadius = Mathf.CeilToInt(radius);
            if (iRadius < 1)
                iRadius = 1;
            int[] data = new int[iRadius];
            int f = 1 - iRadius;
            int ddF_x = 1;
            int ddF_y = -2 * iRadius;
            int x = 0;
            int y = iRadius;
            
            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                data[iRadius - y] = x;
                data[iRadius - x] = y;
            }
            
            Vector2Int center = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
            int x0 = center.x;
            int y0 = center.y - iRadius;
            int y1 = center.y + iRadius - 1;

            for (int _y = 0; _y < data.Length; _y++)
            {
                for (int _x = -data[_y]; _x < data[y]; _x++)
                {
                    yield return new Vector3Int(_x + x0, _y + y0, 0);
                    yield return new Vector3Int(_x + x0, y1 - _y, 0);
                }
            }

        }
    }
}
