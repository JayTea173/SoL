using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace SoL.Serialization
{
    public class PrefabRegistry<T> where T : Object, IIdentifyable
    {
        [SerializeField][InlineProperty]
        protected Dictionary<ulong, T> d = new Dictionary<ulong, T>();

        #if UNITY_EDITOR
        [Button]
        public void RegisterAllPrefabs()
        {
            d.Clear();

            System.Type typeToLoad = null;

            string[] foundAssets = null;
            if (typeof(T).InheritsFrom<Component>())
            {
                typeToLoad = typeof(GameObject);
                foundAssets = UnityEditor.AssetDatabase.FindAssets($"t:gameobject");
            }
            else
            {
                typeToLoad = typeof(T);
                foundAssets = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).GetNiceName()}");
            }

            


            
            foreach (var assetGuid in foundAssets)
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuid), typeToLoad);
                if (prefab == null)
                {
                    Debug.LogError("Failed to load asset: " + assetGuid);
                    continue;
                }

                if (typeToLoad == typeof(GameObject))
                {
                    var go = (GameObject)prefab;
                    var i = go.GetComponent<T>();
                    if (d == null)
                        d = new Dictionary<ulong, T>();
                    if (i != null)
                    {
                        if (d.ContainsKey(i.PrefabId))
                        {
                            Debug.LogError($"Duplicate key: {go.name} and {d[i.PrefabId].name}");
                        }
                        d.Add(i.PrefabId, i);
                    }
                }
                else
                {
                    var i = (IIdentifyable)prefab;
                    if (d == null)
                        d = new Dictionary<ulong, T>();
                    if (d.ContainsKey(i.PrefabId))
                    {
                        Debug.LogError($"Duplicate key: {prefab.name} and {d[i.PrefabId].name}");
                    }
                    d.Add(i.PrefabId, (T)prefab);
                }
            }
        }
        
        #endif

        public bool TryGet(ulong prefabId, out T s)
        {
            if (d == null)
                Debug.LogError("D IS NULL=!!");
            return d.TryGetValue(prefabId, out s);
        }

    }


}