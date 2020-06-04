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
        public uint[] xpNext;

        [Header("Attributes")]
        public byte strength;
        public byte agility, constitution, intelligence, wisdom;

        [Header("Equipment")]
        public ushort equipmentDefense;
        public ushort equipmentMagicDefense;
        public ushort weaponDamage;

        [Header("Calculated Stats")]
        public ushort attack;
        public byte hitChance;
        public ushort defense;
        public byte evadeChance;
        public ushort magicDefense;

        [Header("Level Attribute Gain")]
        public float strengthGain;
        public float agilityGain, constitutionGain, intelligenceGain, wisdomGain;
        public float maxHpGain;
        public float maxMpGain;

        protected const byte maxLevel = 99;


        protected bool sprint;

        public Audio.Soundbank attackSound;

        public bool sprinting
        {
            get
            {
                return sprint;
            }
            set
            {
                sprint = value;
                charging = !sprint;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            CalculateStats();

        }

        public override bool Charging
        {
            get
            {
                if (sprinting)
                    return false;
                return base.Charging;
            }
        }

        protected override void PlayMovementAnimation()
        {
            if (sprinting)
            {
                if (!Animation.SetAnimation("Sprint", false))
                    base.PlayMovementAnimation();
            }
            else
                base.PlayMovementAnimation();
        }

        protected virtual void CalculateStats()
        {
            attack = (ushort)(strength + weaponDamage);
            hitChance = (byte)(75 + agility / 4);
            defense = (ushort)(constitution + equipmentDefense);
            evadeChance = (byte)(equipmentDefense + agility / 4);
            magicDefense = (ushort)(wisdom + equipmentMagicDefense);
        }

        public override int Damage(int amount, IDamageSource damageSource)
        {
            byte rnd = (byte)UnityEngine.Random.Range(0, 100);
            if (rnd < evadeChance)
            {
                Animation.SetAnimation("Dodge", true);
                UI.DamageNumber.Display(transform, "dodge");
                return 0;
            }

            if (damageSource is CreatureActor)
            {
                CreatureActor creature = damageSource as CreatureActor;
                Move(-creature.movementDirection);
            }
           
            int dmg = System.Math.Max(amount - (int)defense, 0);
            return base.Damage(dmg, damageSource);
        }

        public override int GetDamageDealt()
        {
            
            return Mathf.RoundToInt(attack * (currentAnimationAttackCharge * currentAnimationAttackCharge));
        }

        protected override void OnUpdate()
        {
            if (sprinting)
            {

                AttackCharge -= Time.deltaTime * 0.2f;
                if (AttackCharge <= 0f)
                {
                    AttackCharge = 0f;
                    sprinting = false;
                }
                if (movement.sqrMagnitude <= 0f)
                    sprinting = false;

                if (!sprinting)
                    charging = true;
            }

            base.OnUpdate();
        }

        public override void Attack()
        {
            base.Attack();
            AudioSource.PlayClipAtPoint(attackSound.GetRandom(), transform.position, 1f);
            
        }

        private void LevelUp()
        {
            float l = (float)level;
            if (level > 1) {
                strength -= (byte)Mathf.RoundToInt((l-1) * strengthGain);
                agility -= (byte)Mathf.RoundToInt((l - 1) * agilityGain);
                constitution -= (byte)Mathf.RoundToInt((l - 1) * constitutionGain);
                intelligence -= (byte)Mathf.RoundToInt((l - 1) * intelligenceGain);
                wisdom -= (byte)Mathf.RoundToInt((l - 1) * wisdomGain);
                hpMax -= (byte)Mathf.RoundToInt((l - 1) * maxHpGain);
            }
            strength += (byte)Mathf.RoundToInt(l * strengthGain);
            agility += (byte)Mathf.RoundToInt(l * agilityGain);
            constitution += (byte)Mathf.RoundToInt(l * constitutionGain);
            intelligence += (byte)Mathf.RoundToInt(l * intelligenceGain);
            wisdom += (byte)Mathf.RoundToInt(l * wisdomGain);
            hpMax += (byte)Mathf.RoundToInt(l * maxHpGain);
            hp = hpMax;
            level++;

        }

        protected void AwardXP(uint amount)
        {
            xp += amount;
            int ogLevel = level;
            while (xp > xpNext[level - 1] && level < 99)
            {
                LevelUp();
                
            }

            if (level != ogLevel)
            {
                UI.DamageNumber.Display(transform, "Lvl UP!");
                CalculateStats();
            }
        }

        public override void OnKill(IDamagable target)
        {
            base.OnKill(target);
            if (target is CreatureActor)
            {
                CreatureActor creature = target as CreatureActor;
                AwardXP(creature.ExpAward);

            }
        }
    }
}
