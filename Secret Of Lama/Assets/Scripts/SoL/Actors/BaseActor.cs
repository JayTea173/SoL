using SoL.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BaseActor : MonoBehaviour, IDamagable, IDamageSource
    {
        public enum Facing
        {
            Left,
            Right,
            Down,
            Up
        }

        protected Facing facing = Facing.Down;

        [SerializeField]
        protected EnumTeam team;

        public EnumTeam Team
        {
            get
            {
                return team;
            }
        }

        public bool IsEnemy(EnumTeam other)
        {
            return team != other;
        }

        public bool IsEnemy(BaseActor other)
        {
            return team != other.Team;
        }

        [SerializeField]
        protected int hp;

        public int Hitpoints
        {
            get
            {
                return hp;
            }
        }

        [SerializeField]
        protected int hpMax;

        public int HitpointsMax
        {
            get
            {
                return hpMax;
            }
        }

        private SpriteAnimationBehaviour animation;
        private float attackCharge = 1f;
        private bool charging = false;
        protected float currentAnimationAttackCharge = 0f;
        protected float timeOfDeath = 0f;
        private Physics.QuadTree.Agent physicsAgent;
        public Physics.QuadTree.Agent PhysicsAgent
        {
            get
            {
                return physicsAgent;
            }
        }

        public bool Charging
        {
            get { return charging; }
        }

        public delegate int OnDamageTaken(int amount, IDamageSource source);
        public delegate int OnDamageDealt(int amount, IDamagable target);

        public OnDamageTaken onDamageTaken, onHealed;
        public OnDamageDealt onDamageDealt;

        public float AttackCharge
        {
            get
            {
                return attackCharge;
            }
        }

        [SerializeField]
        private float maxAttackCharge = 1f;

        public float MaxAttackCharge
        {
            get
            {
                return maxAttackCharge;
            }
        }

        [SerializeField]
        private float attackChargeSpeed = .5f;


        public Vector3 position;


        public SpriteAnimationBehaviour Animation
        {
            get
            {
                return animation;
            }
        }

        public virtual bool IsDead
        {
            get
            {
                return hp <= 0;
            }
        }

        public virtual int Damage(int amount, IDamageSource damageSource)
        {
            if (animation.GetCurrentFrameFlags().HasFlag(SpriteAnimationBehaviour.SpriteFrame.Flags.INVULNERABLE))
                return 0;

            if (amount > hpMax)
                amount = hpMax;
            else if (amount > hp)
                amount = hp;



            hp -= amount;
            if (amount > 0)
            {
                if (onDamageTaken != null)
                    amount = onDamageTaken(amount, damageSource);
            }
            else if (amount < 0)
            {
                if (onHealed != null)
                    amount = onHealed(amount, damageSource);
            }

            UI.DamageNumber.Display(transform, amount);



            if (IsDead)
            {
                timeOfDeath = Time.time;
                animation.SetAnimation("Death", true);
            }
            else
                animation.SetAnimation("Hurt");



            return amount;
        }

        public void SortZ()
        {
            position.z = position.y * 0.01f;
        }

        public Vector2 TransformForwardX(Vector2 d)
        {
            Vector2 v = d;
            v.x *= movement.x;
            v.y += movement.y * d.x;
            return v;
        }

        public void UpdateAnimation()
        {

        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Engine.QuadTree.RemoveAgent(physicsAgent);
        }

        protected virtual void Initialize()
        {
            animation = GetComponent<SpriteAnimationBehaviour>();
            position = transform.position;

            physicsAgent = new Physics.QuadTree.Agent(transform, GetComponent<SpriteRenderer>().sprite.bounds, Engine.QuadTree);
            physicsAgent.ab = gameObject.AddComponent<Physics.QuadTreeAgentBehaviour>();
        }

        protected Vector2 movement;


        public bool CanMove
        {
            get
            {
                return !Animation.GetCurrentFrameFlags().HasFlag(SpriteAnimationBehaviour.SpriteFrame.Flags.MOVEMENT_BLOCKED) && !IsDead;
            }
        }



        public void Move(Vector2 move)
        {
            this.movement = move;
            if (move.sqrMagnitude > 0f)
            {
                transform.hasChanged = true;
                if (move.x != 0f)
                {
                    SetFacing(move.x > 0f ? BaseActor.Facing.Right : BaseActor.Facing.Left);
                    if (!Animation.SetAnimation("Walk_Horizontal", false))
                        Animation.SetAnimation(1);
                }
                else if (move.y != 0f)
                {
                    SetFacing(move.y > 0f ? BaseActor.Facing.Up : BaseActor.Facing.Down);


                    if (!Animation.SetAnimation(move.y > 0f ? "Walk_Up" : "Walk_Down", false))
                        if (!Animation.SetAnimation("Walk_Vertical", false))
                            Animation.SetAnimation(1);


                }
            }
            else
            {
                switch (facing)
                {
                    case Facing.Down:
                    case Facing.Up:
                        if (!Animation.SetAnimation("Idle_Vertical", false))
                            Animation.SetAnimation(0);
                        break;
                    case Facing.Left:
                    case Facing.Right:
                        if (!Animation.SetAnimation("Idle_Horizontal", false))
                            Animation.SetAnimation(0);

                        break;
                }
                transform.position = Engine.AlignToPixelGrid(transform.position);
            }

            if (move.x > 0f)
                transform.eulerAngles = new Vector3(0f, 0f, 0f);
            else if (move.x < 0f)
                transform.eulerAngles = new Vector3(180f, 0f, 180f);

        }

        public Vector2 TransformDirection(Vector2 originOffset)
        {
            float angle = Mathf.Atan2(movement.y, movement.x);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private void Update()
        {
            SortZ();

            if (charging && !animation.GetCurrentFrameFlags().HasFlag(SpriteAnimationBehaviour.SpriteFrame.Flags.CHARGING_BLOCKED))
            {
                attackCharge += Time.deltaTime * attackChargeSpeed;
                if (attackCharge >= maxAttackCharge)
                {
                    attackCharge = maxAttackCharge;
                    charging = false;
                }
            }

            if (animation != null)
                animation.Advance(Time.deltaTime);

            if (IsDead)
            {
                float timeDead = Time.time - timeOfDeath;
                if (timeDead > 10f)
                    Destroy(gameObject);
            }
        }



        private void LateUpdate()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                Engine.QuadTree.PlaceAgent(physicsAgent);
            }
        }

        protected Vector3 GetFacingDirectionVector()
        {
            switch (facing)
            {
                case Facing.Left:
                    return Vector3.left;
                case Facing.Right:
                    return Vector3.right;
                case Facing.Down:
                    return Vector3.down;
                case Facing.Up:
                    return Vector3.up;
            }
            return Vector3.zero;
        }

        public void SetFacing(Facing facing)
        {
            this.facing = facing;
        }


        public virtual int GetDamageDealt()
        {
            return Mathf.RoundToInt(8f * (currentAnimationAttackCharge * currentAnimationAttackCharge));
        }

        public virtual void Attack()
        {
            currentAnimationAttackCharge = attackCharge;
            attackCharge = 0f;
            charging = true;

            Animation.SetAnimation("Attack", true);

            //Engine.quadTree.GetAgents();
        }
    }
}
