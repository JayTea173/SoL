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
    public class Inventory : MonoBehaviour, ISerializable
    {
        public List<Weapon> weapons;

        public Weapon GetNextWeapon(Weapon last)
        {
            if (last == null)
                return weapons[0];
            return weapons[(weapons.IndexOf(last) + 1) % weapons.Count];
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(weapons.Count);
            foreach (var w in weapons)
            {
                bw.Write(w.PrefabId);
                w.Serialize(bw);
            }
        }

        public void Deserialize(BinaryReader br)
        {
            int num = br.ReadInt32();
            weapons.Clear();
            for (int i = 0; i < num; i++)
            {
                ulong pid = br.ReadUInt64();
                if (!Engine.Instance.itemRegistry.TryGet(pid, out var item))
                {
                    Debug.LogError("NO WEAPON FOUND");
                    return;
                }
                weapons.Add((Weapon)item);
                item.Deserialize(br);
            }
        }
    }
}
