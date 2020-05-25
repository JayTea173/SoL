using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoL
{
    public class Engine : MonoBehaviour
    {
        private const string gameObjectName = "Engine";
        private static Engine instance;
        public static Engine Instance
        {
            get
            {
                return instance;
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

        }
    }
}
