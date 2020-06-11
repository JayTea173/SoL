using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Items
{
    public class Inventory : MonoBehaviour
    {
        public List<Weapon> weapons;

        public Weapon GetNextWeapon(Weapon last)
        {
            if (last == null)
                return weapons[0];
            return weapons[(weapons.IndexOf(last) + 1) % weapons.Count];
        }
    }
}
