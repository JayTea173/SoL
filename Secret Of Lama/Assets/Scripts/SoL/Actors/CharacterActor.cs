using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class CharacterActor : BaseActor
    {
        [Header("Level & XP")]
        public byte level;
        public uint xp;
        public uint xpNext;

        [Header("Attributes")]
        public byte strength;
        public byte agility, constitution, intelligence, wisdom;

        [Header("Calculated Stats")]
        public ushort attack;
        public byte hitChance;
        public ushort defense;
        public byte evadeChance;
        public ushort magicDefense;
    }
}
