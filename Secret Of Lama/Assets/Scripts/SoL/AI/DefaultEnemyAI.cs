using SoL.Actors;
using SoL.Animation;
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

        [Header("Attack")]
        public float attackRange = 4f;
        public float attackAimYOffset = 0f;

        Vector2 moveDirection = Vector3.right;

        float aiTime = 0f;
        float processedTime = 0f;

        BaseActor actor;
        SpriteAnimationBehaviour animation;

        protected BaseActor target;

        private void Awake()
        {
            actor = GetComponent<BaseActor>();
        }

        private void Update()
        {
            aiTime += Time.deltaTime;

            bool canMove = actor.CanMove;



            while (aiTime - processedTime > .2f)
            {
                processedTime += 1f / 15f;

                if (canMove)
                {
                    if (target == null)
                    {
                        if (UnityEngine.Random.value < 0.65f)
                        {
                            moveDirection = UnityEngine.Random.insideUnitCircle.normalized;
                        }
                    }
                    else
                    {
                        var diff = ((target.transform.position + target.PhysicsAgent.b.center) - transform.position);
                        moveDirection = diff.normalized;

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
                actor.Move(moveDirection);

            if (target == null)
            {
                var targets = Engine.QuadTree.GetAgentsInRange(transform.position, 32f);
                targets = targets.Where((t) => t != actor.PhysicsAgent);
                targets = targets.Where((t) =>
                {
                    var a = t.t.GetComponent<BaseActor>();
                    if (a == null)
                        return false;
                    return a.IsEnemy(actor.Team);
                });

                if (targets.Count() > 0)
                    target = targets.First().t.GetComponent<BaseActor>();
            }

        }

    }
}
