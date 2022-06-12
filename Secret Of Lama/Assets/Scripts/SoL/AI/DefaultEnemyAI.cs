using SoL.Actors;
using SoL.Animation;
using SoL.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoL.UI;
using UnityEngine;

namespace SoL.AI
{
    [RequireComponent(typeof(BaseActor))]
    [RequireComponent(typeof(SpriteAnimationBehaviour))]
    public class DefaultEnemyAI : MonoBehaviour
    {
        [Serializable]
        public class Attack
        {
            public string[] animationNames = new string[] { "Attack" };
            public float chargeCost = 1f;
            public float minChargeUse = 0f;
            public float rangeMin = 0f;
            public float rangeMax = 2f;
        }

        [Header("Behaviour")]
        [Range(0f, 1f)]
        public float agressiveness = 0.5f;
        [Range(0f, 1f)]
        public float annoyance = 0.1f;
        [Range(0f, 1f)]
        public float caution = 0.5f;
        [Range(1f, 32f)]
        public float targetingRadius = 16f;

        [Header("Attack")]
        public List<Attack> attacks;

        [Range(0.1f, 2f)]
        public float pathingTargetLeeway = 0.5f;

        Vector2 moveDirection = Vector3.zero;

        float aiTime = 0f;
        float processedTime = 0f;

        BaseActor actor;
        SpriteAnimationBehaviour animation;

        protected Path pathToTarget = null;

        protected BaseActor target;

        public DefaultEnemyAI[] squadMates;

        public BaseActor Target
        {
            get
            {
                return target;
            }
        }

        private void Awake()
        {
            actor = GetComponent<BaseActor>();

            actor.onDamageTaken += Revenge;
        }

        protected int Revenge(int amount, IDamageSource source)
        {
            if (source is BaseActor)
            {
                SetTarget(source as BaseActor);
               
            }
            return amount;
        }

        protected void UpdatePathing()
        {
            if (pathToTarget != null)
            {
                pathToTarget.DebugDraw();
                if (target.IsMoving)
                {
                    var targetDiff = (target.transform.position - pathToTarget.Target).magnitude;
                    if (targetDiff > 0.5f)
                    {
                        pathToTarget = Pathfinder.GetPath(transform.position, target.transform.position);
                        if (pathToTarget == null)
                            target = null;
                    }
                }
                if (pathToTarget != null)
                    if (pathToTarget.Update(transform.position, pathingTargetLeeway))
                        pathToTarget = null;
            }
            else if (target != null)
            {
                if (Time.frameCount % 30 == 0)
                    pathToTarget = Pathfinder.GetPath(transform.position, target.transform.position);

            }
        }

        private void Update()
        {
            
            aiTime += Time.deltaTime;

            bool canMove = actor.CanMove;

            UpdatePathing();


            while (aiTime - processedTime > .2f)
            {
                processedTime += 1f / 15f;

                UpdatePathing();

                if (canMove)
                {
                    if (target == null)
                    {   /*
                        if (UnityEngine.Random.value < 0.65f)
                        {
                            moveDirection = UnityEngine.Random.insideUnitCircle.normalized;
                        }
                        */
                    }
                    else
                    {
                        //var diff = ((target.transform.position + target.PhysicsAgent.b.center) - transform.position);
                        Vector3 targetPosition = pathToTarget != null ? pathToTarget.GetCurrentTargetPosition() : (target.transform.position + target.PhysicsAgent.b.center);

                        var diff = targetPosition - transform.position;
                        var diffToTarget = (target.transform.position + target.PhysicsAgent.b.center) - transform.position;
                        moveDirection = diff.normalized;

                        //Debug.Log("Target dir: " + moveDirection + " :: " + transform.position + " -> " + pathToTarget.GetCurrentTargetPosition());

                        bool attacking = false;
                        Attack bestAttack = null;

                        float d = diffToTarget.magnitude;

                        foreach (var attack in attacks)
                        {
                            if (d <= attack.rangeMax && d > attack.rangeMin)
                            {
                                bestAttack = attack;
                                break;
                            }
                        }

                        string pickedAnim = null;
                        if (bestAttack != null && !DialogUI.Instance.visible)
                        {
                            if (UnityEngine.Random.value < 0.65f * agressiveness)
                            {
                                if (actor.Charging)
                                {
                                    if (UnityEngine.Random.value < annoyance)
                                        attacking = true;
                                    else if (UnityEngine.Random.value < caution)
                                        moveDirection = -((target.transform.position + target.PhysicsAgent.b.center + Vector3.up * 0f) - transform.position).normalized;
                                }
                                else
                                    attacking = true;
                            }
                            pickedAnim = bestAttack.animationNames[Engine.RandomInt(0, bestAttack.animationNames.Length)];
                        }

                        

                        if (attacking && actor.Animation.CurrentAnimation.name != pickedAnim)
                        {
                            moveDirection = ((target.transform.position + target.PhysicsAgent.b.center + Vector3.up * 0f) - transform.position).normalized;
                            actor.Move(moveDirection);
                            //Debug.Log("Attack with " + pickedAnim);
                            actor.Attack(pickedAnim);
                            actor.AttackCharge -= bestAttack.chargeCost - 1f;
                            canMove = false;
                        }

                    }
                }
            }
            if (canMove)
                if (moveDirection.sqrMagnitude > 0f)
                    actor.Move(moveDirection);

            if (target == null)
            {
                var targets = Engine.QuadTree.GetAgentsInRange(transform.position, targetingRadius);
                targets = targets.Where((t) => t != actor.PhysicsAgent);
                targets = targets.Where((t) =>
                {
                    var a = t.t.GetComponent<BaseActor>();
                    if (a == null)
                        return false;
                    return a.IsEnemy(actor.Team);
                });

                if (targets.Count() > 0)
                {
                    var newTarget = targets.First().t.GetComponent<BaseActor>();
                    Debug.Log("newTarget team : " + newTarget.Team + ", mine: " + actor.Team + ", enemy: " + actor.IsEnemy(newTarget));
                    if (newTarget != target)
                    {
                        Debug.Log(gameObject.name + ": TARGET AQUIRED");
                        SetTarget(targets.First().t.GetComponent<BaseActor>());
                        
                    }

                }
            }

        }

        public void SetTarget(BaseActor target)
        {
            bool newTarget = target != this.target;
            if (newTarget)
            {
                this.target = target;
                pathToTarget = Pathfinder.GetPath(transform.position, target.transform.position);
                foreach (var squadMate in squadMates)
                    squadMate.SetTarget(target);
            }

        }

    }
}
