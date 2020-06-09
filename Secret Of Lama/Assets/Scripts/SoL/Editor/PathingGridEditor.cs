using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace SoL.Editor
{
    using Pathing;
    using UnityEngine.Tilemaps;

    [CustomEditor(typeof(PathingGrid))]
    public class PathingGridEditor : UnityEditor.Editor
    {
        PathingGrid g;

        private void OnEnable()
        {
            g = target as PathingGrid;
        }

        private void ResizeFromTilemapGrid()
        {
            var tms = g.gameObject.GetComponentsInChildren<Tilemap>();
            int xMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMin = int.MaxValue;
            int yMax = int.MinValue;

            foreach (var tm in tms)
            {
                if (tm.cellBounds.xMin < xMin)
                    xMin = tm.cellBounds.xMin;
                if (tm.cellBounds.xMax > xMax)
                    xMax = tm.cellBounds.xMax;
                if (tm.cellBounds.yMin < yMin)
                    yMin = tm.cellBounds.yMin;
                if (tm.cellBounds.yMax > yMax)
                    yMax = tm.cellBounds.yMax;
            }
            g.SetOrigin(xMin, xMax);
            g.Resize(xMax - xMin, yMax - yMin);
        }

        private void Generate()
        {
            
            var o = g.origin;
            var s = g.size;
            int xMin = o.x;
            int yMin = o.y;
            int xMax = xMin + s.x;
            int yMax = yMin + s.y;

            int num = (xMax - xMin) * (yMax - yMin);
            int i = 0;

            for (int y = yMin; y < yMax; y++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Pathing Data", "Baking Pathing data from Tilemaps (" + i + " / " + num + ")", (float)i / (float)num))
                    break;
                for (int x = xMin; x < xMax; x++)
                {

                    i++;
                    World w = World.Instance;
                    if (w == null)
                        w = g.gameObject.GetComponent<World>();
                    if (w == null)
                        Debug.LogError("Unable to locate world to be queried for collisions! Make sure the PathingGrid behaviour is on the same GameObject as your World behaviour!");

                    var rchit = Physics2D.Raycast(new Vector3(x + 0.025f, y + 0.025f, -10f), Vector3.forward, 20f, w.collisionLayer);
                    if (rchit.collider != null)
                    {
                        //Debug.Log("Collided @" + rchit.point + " with " + rchit.collider.gameObject.name + " ! tried to go from " + transform.position + " to " + targetPoint);
                        g[x, y] = true;
                    }
                    else
                        g[x, y] = false;
                }
            }

            EditorUtility.ClearProgressBar();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Resize from Tilemap Grid"))
            {
                ResizeFromTilemapGrid();
            }
            if (GUILayout.Button("Generate from Tilemap Grid"))
            {
                //ResizeFromTilemapGrid();
                Generate();
            }
            if (GUILayout.Button("Test Path"))
            {
                if (Pathfinder.grid == null)
                    Pathfinder.grid = g;
                Pathfinder.GetPath(new Vector3(11.26f, 0.35f, 0f), new Vector3(7.76f, 2.49f, 0f));
            }
        }
    }
}
