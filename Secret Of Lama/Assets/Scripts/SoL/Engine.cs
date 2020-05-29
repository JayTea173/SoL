using SoL.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoL
{
    public class Engine : MonoBehaviour
    {
        public static readonly float pixelsPerUnit = 16f;

        private const string gameObjectName = "Engine";
        private static Engine instance;
        public static Engine Instance
        {
            get
            {
                return instance;
            }
        }

        private QuadTree qt;
        public static QuadTree QuadTree
        {
            get
            {
                return instance.qt;
            }
        }

        public static Canvas MainCanvas
        {
            get
            {
                return StaticData.Instance.mainCanvas;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Init()
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                go = new GameObject(gameObjectName);

            DontDestroyOnLoad(go);

            
            instance = go.GetComponent<Engine>();
            if (instance == null)
                instance = go.AddComponent<Engine>();

            instance.qt = new QuadTree(256, 16);

        }

        public static Vector3 AlignToPixelGrid(Vector3 v)
        {

            Vector2Int pixelPosition = new Vector2Int(Mathf.FloorToInt(v.x * pixelsPerUnit), Mathf.FloorToInt(v.y * pixelsPerUnit));

            return new Vector3(pixelPosition.x / Engine.pixelsPerUnit, pixelPosition.y / Engine.pixelsPerUnit, v.z);
        }
    }
}
