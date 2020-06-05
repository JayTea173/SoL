using SoL.Animation;
using SoL.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Items
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Items/Weapon")]
    public class Weapon : ItemBase
    {
        public ushort damage;
        public Soundbank attackSound;
        public byte upgradeLevel;
        [SerializeField]
        protected ushort damageIncreasePerLevel;
        [Tooltip("Array index are Player Character ids")]
        public CharacterData[] characterData;


        [Serializable]
        public class CharacterData
        {
            public AnimationCollection animations;
            [Range(0f, 9f)][SerializeField]
            protected float skillLevel = 0f;

            public float SkillProgress
            {
                get
                {
                    return skillLevel;
                }
            }

            public int SkillLevel
            {
                get
                {
                    return Mathf.FloorToInt(skillLevel);
                }
            }

            /// <summary>
            /// returns true on level gain.
            /// </summary>
            /// <returns></returns>
            public bool GainSkill()
            {
                int lvl = SkillLevel;
                skillLevel += 0.1f / (float)(1 << lvl);
                return SkillLevel > lvl;
            }
        }
    }
}
