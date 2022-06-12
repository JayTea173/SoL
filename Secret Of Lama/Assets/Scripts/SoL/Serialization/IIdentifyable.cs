using System.IO;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SoL.Serialization
{
    public interface IIdentifyable : ISerializable
    {
        public string Guid { get; set; }
        public ulong PrefabId { get; set; }
    }

    public static class IdUtils
    {
        public static void OnBeforeSerializeGetId(this IIdentifyable i, Object o)
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(o))
            {
                i.PrefabId = CalculateHash(UnityEditor.AssetDatabase.GetAssetPath(o), true);
            }
            else
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(o);
                if (!string.IsNullOrEmpty(path))
                    i.PrefabId = CalculateHash(UnityEditor.AssetDatabase.GetAssetPath(o), true);
            }

            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return;

            if (string.IsNullOrEmpty(i.Guid) || i.Guid.StartsWith("00") && i.Guid.EndsWith("00") || i.Guid == "0")
            {
                CalcGUID(i);
            }
#endif
        }

        public static void CalcGUID(this IIdentifyable i)
        {
            i.Guid = GUID.Generate().ToString();
        }
        
        public static ulong CalculateHash(string read, bool lowTolerance)
        {
            ulong hashedValue = 3074457345618258791ul;
            foreach (var t in read)
            {
                hashedValue += t;
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}