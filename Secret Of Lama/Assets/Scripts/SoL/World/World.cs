﻿using System;
using System.Collections;
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
        public Tilemap[] dataLayers;
        protected List<Tilemap> collidingLayers;
        public LayerMask collisionLayer;

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

            collidingLayers = new List<Tilemap>();
            foreach (var tm in tilemapLayers)
                if (tm.gameObject.layer == (tm.gameObject.layer | 1 << collisionLayer))
                    collidingLayers.Add(tm);
        }

        public Tilemap FindLayerByName(string name)
        {
            foreach (var l in tilemapLayers)
            {
                if (l.name == name)
                    return l;
            }
            return null;
        }

        

        public void SetColorDownsampling(float value, float seconds)
        {
            StartCoroutine(SetColorDownsamplingCoroutine(value, seconds));
        }

        public void SetUVDownsampling(float value, float seconds)
        {
            StartCoroutine(SetUVDownsamplingCoroutine(value, seconds));
        }

        private TilemapRenderer[] GetTileMapRenderers()
        {
            TilemapRenderer[] r = new TilemapRenderer[tilemapLayers.Length];
            for (int i = 0; i < tilemapLayers.Length; i++)
            {
                r[i] = tilemapLayers[i].GetComponent<TilemapRenderer>();
            }
            return r;
        }

        public IEnumerator SetColorDownsamplingCoroutine(float value, float seconds)
        {
            var r = GetTileMapRenderers();
            var mat = r[0].sharedMaterial;
            float v = mat.GetFloat("_ColorDownsamplingSteps");
            value = Mathf.Clamp01(value) * 16f;


            float t = 0f;
            while (t < seconds)
            {

                yield return new WaitForEndOfFrame();
                mat.SetFloat("_ColorDownsamplingSteps", v + (value - v) * (t / seconds));
                t += Time.deltaTime;
            }
            mat.SetFloat("_ColorDownsamplingSteps", value);
            yield break;
        }

        public IEnumerator SetUVDownsamplingCoroutine(float value, float seconds)
        {
            var r = GetTileMapRenderers();
            var mat = r[0].sharedMaterial;
            value = Mathf.Clamp01(value);
            float v = mat.GetFloat("_UVDownsampling");

            float t = 0f;
            while (t < seconds)
            {

                yield return new WaitForEndOfFrame();
                mat.SetFloat("_UVDownsampling", v + (value - v) * (t / seconds));
                t += Time.deltaTime;
            }
            mat.SetFloat("_UVDownsampling", value);
            yield break;
        }



        public IEnumerable<Vector3Int> GetTilesInRadius(int layer, Vector3 point, float radius)
        {
            int iRadius = Mathf.CeilToInt(radius);
            if (iRadius <= 1)
            {
                yield return new Vector3Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), 0);
                yield break;
            }
            else
            {
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

        public IEnumerable<T> GetDataTiles<T>(Vector3 position) where T : TileBase
        {
            position.z = 0f;
            foreach (var l in dataLayers)
            {
                var t = l.GetTile(position.ToInt());
                if (t != null)
                    if (t is T)
                        yield return (T)t;
            }
        }

        public IEnumerable<T> GetTiles<T>(Vector3 position) where T : TileBase
        {
            position.z = 0f;
            foreach (var l in tilemapLayers)
            {

                //var t = l.GetTile<T>(position.ToInt());
                
                var t = l.GetTile(position.ToInt());
                if (t != null)
                    if (t is T)
                        yield return (T)t;
            }
        }
    }
}
