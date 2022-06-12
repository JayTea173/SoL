using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoL.Serialization;
using UnityEngine;

namespace SoL
{
    [Serializable]
    public class PlayerData : ISerializable
    {
        public uint gp = 0;
        public void Serialize(BinaryWriter bw)
        {
            bw.Write(gp);
        }

        public void Deserialize(BinaryReader br)
        {
            gp = br.ReadUInt32();
        }
    }
}
