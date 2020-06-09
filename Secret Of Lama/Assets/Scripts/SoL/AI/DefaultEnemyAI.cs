using SoL.Actors;
using SoL.Animation;
using SoL.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.AI
{
    [RequireComponent(typeof(BaseActor))]
    [RequireComponent(typeof(SpriteAnimationBehaviour))]
    public class DefaultEnemyAI : MonoBehaviour
    {
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
        public float attackRange = 4f;
        public float attackAimYOffset = 0f;

        [Range(0.1f, 2f)]
        public float pathingTargetLeeway = 0.5f;

        Vector2 moveDirection = Vector3.zero;

        float aiTime = 0f;
        float processedTime = 0f;

        BaseActor actor;
        SpriteAnimationBehaviour animation;

        protected Path pathToTarget = null;

        protected BaseActor target;

        private void Awake()
        {
            actor = GetComponent<BaseActor>();
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
                        moveDirection = diff.normalized;

                        //Debug.Log("Target dir: " + moveDirection + " :: " + transform.position + " -> " + pathToTarget.GetCurrentTargetPosition());

                        bool attacking = false;
                        if (diff.magnitude <= attackRange)
                        {

                            if (UnityEngine.Random.value < 0.65f * agressiveness)
                            {
                                if (actor.Charging)
                                {
                                    if (UnityEngine.Random.value < annoyance)
                                        attacking = true;
                                    else if (UnityEngine.Random.value < caution)
                                        moveDirection = -((target.transform.position + target.PhysicsAgent.b.center + Vector3.up * attackAimYOffset) - transform.position).normalized;
                                }
                                else
                                    attacking = true;
                            }
                        }


                        if (attacking && actor.Animation.CurrentAnimation.name != "Attack")
                        {
                            moveDirection = ((target.transform.position + target.PhysicsAgent.b.center + Vector3.up * attackAimYOffset) - transform.position).normalized;
                            actor.Move(moveDirection);
                            actor.Attack();

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
                        target = targets.First().t.GetComponent<BaseActor>();
                        pathToTarget = Pathfinder.GetPath(transform.position, target.transform.position);
                    }

                }
            }

        }

    }
}
