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

        public Facing facing { get; protected set; }


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

        [SerializeField]
        protected int mp;

        public int Mana
        {
            get
            {
                return mp;
            }
        }

        [SerializeField]
        protected int mpMax;

        public int ManaMax
        {
            get
            {
                return mpMax;
            }
        }



        private SpriteAnimationBehaviour animation;
        protected float attackCharge = 1f;
        protected bool charging = false;
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

        public virtual bool Charging
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
            set
            {
                attackCharge = value;
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
            if (animation.GetCurrentFrameFlags().HasFlag(FrameFlags.INVULNERABLE))
                return 0;

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

            int originalAmount = amount;

            if (amount > hpMax)
                amount = hpMax;
            else if (amount > hp)
                amount = hp;


            hp -= amount;

            UI.DamageNumber.Display(transform, originalAmount);



            if (IsDead)
            {
                timeOfDeath = Time.time;
                damageSource.OnKill(this);
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

        public Vector2 movementDirection
        {
            get
            {
                return movement;
            }
        }


        public bool CanMove
        {
            get
            {
                return !Animation.GetCurrentFrameFlags().HasFlag(FrameFlags.MOVEMENT_BLOCKED) && !IsDead;
            }
        }

        public void MoveToCheckingCollision(Vector3 targetPoint)
        {
            float offset = 1f;
            Vector3 delta = targetPoint - transform.position;
            Vector3 vOffset = delta.normalized * offset;
            var rchit = Physics2D.Raycast(transform.position + vOffset, delta.normalized, delta.magnitude);
            if (rchit.collider != null)
            {
                //Debug.Log("Collided @" + rchit.point + " with " + rchit.collider.gameObject.name + " ! tried to go from " + transform.position + " to " + targetPoint);
                Vector3 p = new Vector3(rchit.point.x, rchit.point.y, transform.position.z);
                Vector3 d = (p - transform.position);
                transform.position = p - (d.normalized * (offset + 0.02f));
            }
            else
                transform.position = targetPoint;
        }

        protected virtual void PlayMovementAnimation()
        {
            if (!Animation.SetAnimation("Walk", false))
                Animation.SetAnimation(1);
        }

        public void Move(Vector2 move)
        {
            if (move != Vector2.zero)
                this.movement = move;
            if (move.sqrMagnitude > 0f)
            {
                transform.hasChanged = true;
                if (move.x != 0f)
                    SetFacing(move.x > 0f ? BaseActor.Facing.Right : BaseActor.Facing.Left);
                else if (move.y != 0f)
                    SetFacing(move.y > 0f ? BaseActor.Facing.Up : BaseActor.Facing.Down);

                PlayMovementAnimation();

            }
            else if (CanMove)
            {
                switch (facing)
                {
                    case Facing.Down:
                    case Facing.Up:
                        if (!Animation.SetAnimation("Idle", false))
                            Animation.SetAnimation(0);
                        break;
                    case Facing.Left:
                    case Facing.Right:
                        if (!Animation.SetAnimation("Idle", false))
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

        protected virtual void OnUpdate()
        {
            if (charging && !animation.GetCurrentFrameFlags().HasFlag(FrameFlags.CHARGING_BLOCKED))
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

        private void Update()
        {
            SortZ();
            OnUpdate();
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

        /// <summary>
        /// called when killed by target
        /// </summary>
        /// <param name="target"></param>
        public virtual void OnKill(IDamagable target)
        {

        }
    }
}
