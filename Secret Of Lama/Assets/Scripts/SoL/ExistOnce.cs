using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SoL
{
    public class ExistOnce : MonoBehaviour
    {
        private static readonly List<string> removed = new List<string>();

        public string identifier;

        private void Start()
        {
            if (removed.Contains(identifier))
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!removed.Contains(identifier))
                removed.Add(identifier);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            removed.Clear();
        }

    }
}
