using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace SoL.Editor
{
    [CustomEditor(typeof(Tile), true, isFallback = false)]
    public class TileEditor : UnityEditor.Editor
    {
        Tile t;

        private void OnEnable()
        {
            t = target as Tile;
        }

        public override void OnInspectorGUI()
        {
            // Extract new local position
            Vector3 position = t.transform.GetColumn(3);

            // Extract new local rotation
            Quaternion rotation = Quaternion.LookRotation(
                t.transform.GetColumn(2),
                t.transform.GetColumn(1)
            );

            // Extract new local scale
            Vector3 scale = new Vector3(
                t.transform.GetColumn(0).magnitude,
                t.transform.GetColumn(1).magnitude,
                t.transform.GetColumn(2).magnitude
            );

            EditorGUI.BeginChangeCheck();

            position = EditorGUILayout.Vector3Field("Position", position);
            rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", rotation.eulerAngles));
            scale = EditorGUILayout.Vector3Field("Scale", scale);


            if (EditorGUI.EndChangeCheck())
            {
                t.transform = Matrix4x4.TRS(position, rotation, scale);
            }
            base.OnInspectorGUI();
        }
    }
}
