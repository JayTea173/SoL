using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoL.Serialization
{
    public interface ISerializable
    {
        public void Serialize(BinaryWriter bw);
        public void Deserialize(BinaryReader br);
    }

    public interface ISerializableTile
    {
        public void Serialize(BinaryWriter bw, Vector3Int location, Tilemap tilemap);
        public void Deserialize(BinaryReader br, Vector3Int location, Tilemap tilemap);
    }
}