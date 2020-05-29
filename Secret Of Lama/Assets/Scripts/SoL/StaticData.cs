using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL
{
    public class StaticData : MonoBehaviour
    {
        [Header("UI Prefabs")]
        public GameObject damageNumberPrefab;
        public GameObject damageNumberPrefabLarge;

        public Canvas mainCanvas;

        private static StaticData instance;
        public static StaticData Instance
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
    }
}
