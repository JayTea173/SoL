using SoL.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SoL.Animation;

namespace SoL.Actors
{
    public class CharacterActor : BaseActor
    {
        [Tooltip("Used to keep track of character specific data on other objects")]
        public int id;

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
        public Weapon weapon;

        public ushort weaponDamage
        {
            get
            {
                return weapon.damage;
            }
        }

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

        public override float MaxAttackCharge
        {
            get
            {
                if (weapon != null && weaponCharging && attackCharge != 1f)
                    return (weapon.characterData[id].SkillLevel + 1f);
                return base.MaxAttackCharge;
            }
        }

        public bool weaponCharging;

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

        protected override bool CanDestroyTiles
        {
            get
            {
                return true;
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

        protected override void PlayAttackAnimation()
        {
            if (weapon == null)
                base.PlayAttackAnimation();
            else
            {
                int chargeLevel = Mathf.FloorToInt(attackCharge) - 1;

                if (chargeLevel < 1)
                    Animation.SetAnimation(weapon.characterData[id].animations, "Attack", true);
                else
                {
                    while (!Animation.SetAnimation(weapon.characterData[id].animations, "Charge" + chargeLevel.ToString(), true) && chargeLevel > 0)
                        chargeLevel--;

                    if (chargeLevel < 1)
                        Animation.SetAnimation(weapon.characterData[id].animations, "Attack", true);
                }
            }
            //base.PlayAttackAnimation();
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

            float chargeMultiplier = currentAnimationAttackCharge;
            if (chargeMultiplier <= 1f)
                chargeMultiplier *= chargeMultiplier;
            else
                chargeMultiplier = 1f + Mathf.Floor(chargeMultiplier - 1f) / 2f;

            return Mathf.RoundToInt(attack * chargeMultiplier);
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
            AudioSource.PlayClipAtPoint(weapon.attackSound.GetRandom(), transform.position, 1f);

        }

        private void LevelUp()
        {
            float l = (float)level;
            if (level > 1)
            {
                strength -= (byte)Mathf.RoundToInt((l - 1) * strengthGain);
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

                //onDamageTaken forces the UI to be updated to display a change in hp
                if (onDamageTaken != null)
                    onDamageTaken.Invoke(0, null);
            }
        }

        public override void OnKill(IDamagable target)
        {
            base.OnKill(target);
            if (target is CreatureActor)
            {
                CreatureActor creature = target as CreatureActor;
                AwardXP(creature.ExpAward);
                if (weapon != null)
                {
                    if (weapon.characterData[id].GainSkill())
                    {
                        UI.DamageNumber.Display(transform, "Wpn UP!");
                    }
                }
                PlayerController.Instance.playerData.gp += creature.GpAward;
            }
        }

        //Debug only
        private void OnGUI()
        {
            GUILayout.Label("XP : " + xp + " / " + xpNext[level - 1]);
            GUILayout.Label("Wpn: " + weapon.characterData[id].SkillProgress.ToString("0.00"));
        }
    }
}
