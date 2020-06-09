using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Items
{
    public class WeaponDatabase : MonoBehaviour
    {
        private static WeaponDatabase instance;
        public static WeaponDatabase Instance
        {
            get
            {
                return instance;
            }
        }
        public Weapon[] weapons;

        private void Awake()
        {
            instance = this;
        }
    }
}
