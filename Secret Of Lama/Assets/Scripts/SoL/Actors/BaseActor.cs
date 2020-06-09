using SoL.Animation;
using SoL.Audio;
using SoL.Tiles;
using SoL.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(AudioSource))]
    public class BaseActor : MonoBehaviour, IDamagable, IDamageSource
    {
        public enum Facing
        {
            Left,
            Right,
            Down,
            Up
        }

        public static Facing InvertFacing(Facing facing)
        {
            switch (facing)
            {
                case Facing.Left:
                    return Facing.Right;
                case Facing.Right:
                    return Facing.Left;
                case Facing.Down:
                    return Facing.Up;
                case Facing.Up:
                    return Facing.Down;
            }
            return Facing.Left;
        }

        public Facing facing { get; protected set; }

        protected virtual bool CanDestroyTiles
        {
            get
            {
                return false;
            }
        }


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
            return team != other && other != EnumTeam.TRIGGER && team != EnumTeam.TRIGGER;
        }

        public bool IsEnemy(BaseActor other)
        {
            return IsEnemy(other.team);
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
        protected Rigidbody2D rb;

        public Rigidbody2D Rigidbody
        {
            get
            {
                return rb;
            }
        }

        protected Collider2D collider;
        public Collider2D Collider
        {
            get
            {
                return collider;
            }
        }

        [Header("Sound")]
        public Soundbank soundHurt;
        public Soundbank soundDodge;

        protected AudioSource audioSource;


        [SerializeField]
        protected float attackCharge = 1f;
        protected bool charging = false;
        protected float currentAnimationAttackCharge = 0f;
        protected float timeOfDeath = 0f;

        public float flyingHeight = 0f;

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

        public virtual float MaxAttackCharge
        {
            get
            {
                return 1f;
            }
        }

        public bool RenderShadow
        {
            get
            {
                return shadowRenderer.enabled;
            }
            set
            {
                shadowRenderer.enabled = value;
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

        protected SpriteRenderer shadowRenderer;


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

            UpdateLiquid();

            return amount;
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
            rb = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();
            shadowRenderer = GetComponent<SpriteRenderer>();
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


        public virtual bool CanMove
        {
            get
            {
                if (Animation == null)
                    return false;
                return !Animation.GetCurrentFrameFlags().HasFlag(FrameFlags.MOVEMENT_BLOCKED) && !IsDead;
            }
        }

        public virtual bool CanAttack
        {
            get
            {
                return canAttack;
            }
        }

        protected bool canAttack = true;

        private bool moving = false;
        public bool IsMoving
        {
            get
            {
                return moving;
            }
        }

        public void LookAt(Vector3 position)
        {
            float dX = position.x - transform.position.x;
            float dY = position.y - transform.position.y;
            //if (dX > dY)
            //{
            SetFacing(dX > 0f ? Facing.Right : Facing.Left);
            /* }
             else
             {
                 SetFacing(dY > 0f ? Facing.Up : Facing.Down);
             }*/

            movement = GetFacingDirectionVector();

            Animation?.SetAnimation(0, false);
            Move(GetFacingDirectionVector());
            Move(Vector2.zero);
        }

        protected bool colliding = false;

        public bool MoveToCheckingCollision(Vector3 targetPoint)
        {/*
            Vector2 delta = (Vector2)(targetPoint - transform.position);
            Vector2 vOffset = delta.normalized;
            if (animation.spriteRenderer == null)
                return true;

            Vector3 size = animation.spriteRenderer.bounds.size * 0.02f;
            var rchit = Physics2D.BoxCast((Vector2)transform.position, size, 0f, delta.normalized, vOffset.magnitude * 0.5f, World.Instance.collisionLayer);
            if (rchit.collider != null)
            {
                colliding = true;
                var rchit0 = Physics2D.Raycast((Vector2)transform.position, delta.x > 0f ? Vector3.right : Vector3.left, delta.x, World.Instance.collisionLayer);
                if (rchit0.collider == null)
                    transform.position = new Vector3(targetPoint.x - delta.x * 0.51f, transform.position.y, transform.position.z);
                //else
                //    transform.position = new Vector3(targetPoint.x - delta.x * 1f, transform.position.y, transform.position.z);

                
                rchit0 = Physics2D.Raycast((Vector2)transform.position, delta.y > 0f ? Vector3.up : Vector3.down, delta.y, World.Instance.collisionLayer);
                if (rchit0.collider == null)
                    transform.position = new Vector3(transform.position.x, targetPoint.y - delta.y * 0.51f, transform.position.z);
                    

            }
            else
            {
                transform.position = targetPoint;
                colliding = false;
                return true;
            }
            return false;
            */

            if (rb != null)
            {
                rb.MovePosition(targetPoint);
            }
            return true;


        }

        protected virtual void PlayMovementAnimation()
        {
            if (Animation == null)
                return;
            if (!Animation.SetAnimation("Walk", false))
                Animation.SetAnimation(1);
        }

        public virtual void PlayAttackAnimation()
        {
            if (!Animation.SetAnimation("Attack", false))
                Animation.SetAnimation(0);
        }

        public void Move(Vector2 move)
        {
            if (move.sqrMagnitude > 0f)
            {
                moving = true;
                this.movement = move;
                transform.hasChanged = true;
                if (move.x != 0f)
                    SetFacing(move.x > 0f ? BaseActor.Facing.Right : BaseActor.Facing.Left);
                else if (move.y != 0f)
                    SetFacing(move.y > 0f ? BaseActor.Facing.Up : BaseActor.Facing.Down);

                PlayMovementAnimation();

            }
            else if (CanMove)
            {
                moving = false;
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
                if (!colliding)
                    transform.position = Engine.AlignToPixelGrid(transform.position);
            }

            if (move.x > 0f)
                Animation.spriteRenderer.flipX = false;
            else if (move.x < 0f)
                Animation.spriteRenderer.flipX = true;

        }


        public virtual void HandleDamageFrame(SpriteFrame frame, List<IDamagable> targetsHitWithThisAnimation, bool isStartOfFrame)
        {
            Vector2 offset = TransformDirection(frame.damage.originOffset);
            Vector3 center = transform.position + PhysicsAgent.b.center + (Vector3)offset;
            var hits = Engine.QuadTree.GetAgentsInRange(center, frame.damage.radius).Where((h) =>
            {
                if (h.t == transform)
                    return false;
                var a = h.t.GetComponent<IDamagable>();
                return IsEnemy(a.Team);
            });

            foreach (var hit in hits)
            {
                var damageable = hit.t.GetComponent<IDamagable>();

                if (!damageable.IsDead)
                {
                    if (!targetsHitWithThisAnimation.Contains(damageable))
                    {
                        targetsHitWithThisAnimation.Add(damageable);
                        damageable.Damage(Mathf.FloorToInt(frame.damage.value * GetDamageDealt()), this);
                        audioSource.PlayOneShot(soundHurt.GetRandom());
                    }
                }
            }

            if (isStartOfFrame)
            {
                if (CanDestroyTiles)
                {
                    var positions = World.Instance.GetTilesInRadius(0, center, Mathf.Max(frame.damage.radius, 0f));
                    var tm = World.Instance.tilemapLayers[0];
                    foreach (var p in positions)
                    {
                        var tile = tm.GetTile(p);
                        if (tile is DestructibleTile)
                        {
                            (tile as DestructibleTile).Kill(p, tm);
                        }
                    }

                }
            }
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
                if (attackCharge >= MaxAttackCharge)
                {
                    attackCharge = MaxAttackCharge;
                    charging = false;
                }
            }

            if (animation != null)
                animation.Advance(Time.deltaTime);

            if (IsDead)
            {
                float timeDead = Time.time - timeOfDeath;
                rb.isKinematic = true;
                if (timeDead > 10f)
                    Destroy(gameObject);
            }

            if (moving)
            {
                UpdateLiquid();

            }
        }

        public void UpdateLiquid()
        {
            var liquids = World.Instance.GetDataTiles<LiquidTile>(transform.position);
            float height = -100f;
            foreach (var liquid in liquids)
            {
                height = -0.57f;
            }
            if (animation != null)
                animation.spriteRenderer.material.SetFloat("_TintHeight", height);
        }

        private void Update()
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if attack went off</returns>
        public virtual void Attack()
        {
            if (!CanAttack)
                return;
            currentAnimationAttackCharge = attackCharge;

            PlayAttackAnimation();
            attackCharge = 0f;
            charging = true;

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
