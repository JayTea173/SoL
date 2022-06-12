using SoL.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SoL.Animation;
using SoL.Serialization;
using UnityEngine.Tilemaps;
using SoL.Tiles;
using SoL.UI;

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
        public Inventory inventory;

        public ushort weaponDamage
        {
            get
            {
                if (weapon == null)
                    return 0;
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

        private IEnumerable<TileBase> tilesStandingOn;
        private Vector3 tileEnterPosition;


        protected bool sprint;
        public float timeSprintStart = 0f;

        public bool sprinting
        {
            get
            {
                return sprint;
            }
            set
            {
                if (!sprinting && value)
                    timeSprintStart = Time.time;
                
                sprint = value;
                charging = !sprint;
            
            }
        }

        public override bool CanMove
        {
            get
            {
                return base.CanMove && timeLeftRooted <= 0f;
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
        
        public override void Serialize(BinaryWriter bw)
        {
            base.Serialize(bw);
            bw.Write(level);
            bw.Write(xp);
            
            inventory.Serialize(bw);
        }

        public override void Deserialize(BinaryReader br)
        {
            base.Deserialize(br);
            br.ReadByte();
            br.ReadUInt32();
            ulong weaponPrefabId = br.ReadUInt64();
            inventory.Deserialize(br);
            weapon = inventory.weapons.FirstOrDefault();

        }

        public void SwapWeapons()
        {
            weapon = inventory.GetNextWeapon(weapon);
            CalculateStats();
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

        public override void PlayAttackAnimation(string animationName = "Attack")
        {
            if (weapon == null)
                return;
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
            if (timeLeftRooted <= 0f)
            {
                byte rnd = (byte)UnityEngine.Random.Range(0, 100);
                if (!this.IsDead && rnd < evadeChance || (this.sprinting && (Time.time - timeSprintStart) < 0.35f))
                {
                    Animation.SetAnimation("Dodge", true);
                    UI.DamageNumber.Display(transform, "dodge");
                    audioSource.PlayOneShot(soundDodge.GetRandom());
                    this.attackCharge = Mathf.Clamp(this.attackCharge + 0.33f, 0f, this.MaxAttackCharge);
                    return 0;
                }

                if (damageSource is CreatureActor)
                {
                    CreatureActor creature = damageSource as CreatureActor;
                    Move(-creature.movementDirection);
                }
            } else
                Debug.LogError("ROOTED!");
            
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

        private Vector3Int lastTilePosition;
        private Vector3 lastPosition;

        private float timeLeftRooted = 0f;

        public void RootForSeconds(float seconds, bool playIdle = true)
        {
            timeLeftRooted = seconds;
            sprint = false;
            if (playIdle)
                Animation.SetAnimation("Idle", true);
        }

        public override float AttackSpeedCharge
        {
            get
            {
                if (weapon == null)
                return base.AttackSpeedCharge;
                return weapon.chargeSpeedOverride;
            }
        }

        protected override void OnUpdate()
        {
            if (timeLeftRooted > 0f)
            {
                timeLeftRooted -= Time.deltaTime;
                if (CanMove)
                    if (attackCharge < 1f)
                        charging = true;
            }

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

            if (IsMoving)
            {

                if (lastTilePosition != transform.position.ToInt())
                {
                    //Debug.Log("Tile position changed to " + transform.position.ToInt());
                    lastTilePosition = transform.position.ToInt();
                    tileEnterPosition = transform.position;

                    this.tilesStandingOn = World.Instance.GetTiles<TileBase>(transform.position.ToInt());
                    //Debug.Log("Now standing on " + tilesStandingOn.Count() + " tiles");


                }
                foreach (var t in tilesStandingOn)
                {
                    if (t is StairTile)
                    {
                        var stair = t as StairTile;
                        Vector3Int v0 = transform.position.ToInt();
                        Vector3 localTilePosition = transform.position - new Vector3(v0.x, v0.y, v0.z);

                        if (localTilePosition.y < stair.localStairStartMax && localTilePosition.y >= stair.localStairStartMin)
                        {
                            Vector3 lastLocalTilePosition = lastPosition - lastTilePosition;

                            if (stair.upDirection.x != 0f)
                            {
                                float sizePerStep = (1f / (float)stair.numSteps);
                                int step = Mathf.FloorToInt(localTilePosition.x / sizePerStep);
                                int lastStep = Mathf.FloorToInt((lastPosition.x - lastTilePosition.x) / sizePerStep);
                                if (step != lastStep)
                                {
                                    int deltaStep = step - lastStep;
                                    //Vector3 oldPosition = transform.position;
                                    transform.Translate(0f, deltaStep * sizePerStep * stair.upDirection.x, 0f);
                                    //if (!MoveToCheckingCollision(transform.position + Vector3.up * deltaStep * sizePerStep * stair.upDirection.x))
                                    //   transform.position = oldPosition;
                                    RootForSeconds(1f / 60f * 5f);
                                    if (stair.stepSound != null)
                                    {

                                        if (audioSource != null)
                                        {
                                            Debug.LogError("PLAY STEP SOUND " + gameObject.name);
                                            audioSource.PlayOneShot(stair.stepSound.GetRandom());
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }



            base.OnUpdate();
            lastPosition = transform.position;

        }

        public override void Attack(string animationName = "Attack")
        {

            Vector2 offset = TransformDirection(Vector2.right);
            Vector3 center = transform.position + PhysicsAgent.b.center + PhysicsAgent.b.extents.x * (Vector3)offset;
            var hits = Engine.QuadTree.GetAgentsInRange(center, 2f).Where((h) =>
            {
                if (h.t == transform)
                    return false;
                var a = h.t.GetComponent<IDamagable>();
                //if (a == null)
                return true;
                //Debug.Log("Potential: " + (a as BaseActor).gameObject.name + " isEnemy: " + IsEnemy(a.Team).ToString());
                //return !IsEnemy(a.Team);
            });


            foreach (var hit in hits)
            {
                var dialogPartner = hit.t.GetComponent<DialogPartner>();

                if (dialogPartner != null)
                {
                    if (dialogPartner.dialog != null)
                    {
                        if (!UI.DialogUI.Instance.visible)
                            dialogPartner.StartDialog(GetComponent<DialogPartner>());
                        return;
                    }
                }
            }

            base.Attack();
            if (audioSource != null && weapon != null)
            {
                audioSource.PlayOneShot(weapon.attackSound.GetRandom(), 1f);
            }

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
            while (xp > xpNext[level] && level < 99)
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
        /*
        private void OnGUI()
        {
            GUILayout.Label("XP : " + xp + " / " + xpNext[level]);
            GUILayout.Label("Wpn: " + weapon.characterData[id].SkillProgress.ToString("0.00"));
        }
        */
    }
}
