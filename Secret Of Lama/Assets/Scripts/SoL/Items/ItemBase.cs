using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoL.Serialization;
using UnityEngine;

namespace SoL.Items
{
    public abstract class ItemBase : ScriptableObject, IIdentifyable, ISerializationCallbackReceiver
    {
        [SerializeField][Sirenix.OdinInspector.ReadOnly]
        private string guid;
        
        [SerializeField][Sirenix.OdinInspector.ReadOnly]
        private ulong prefabId;

        public string Guid
        {
            get => guid; 
            set => guid = value; }

        public ulong PrefabId
        {
            get => prefabId;
            set => prefabId = value;
        }

        public virtual void Serialize(BinaryWriter bw)
        {
            
        }

        public virtual void Deserialize(BinaryReader br)
        {

        }

        public void OnBeforeSerialize()
        {
            this.OnBeforeSerializeGetId(this);
        }

        public void OnAfterDeserialize()
        {
            
        }

        #if UNITY_EDITOR
        private void OnEnable()
        {
            OnBeforeSerialize();
        }
        #endif
    }
}
