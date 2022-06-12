using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehaviour : MonoBehaviour, IDamageSource
    {
        public int damageDealt;
        public GameObject owner;
        public float startSpeed;
        public float lifeTime;
        public float spread = 0f;
        public bool initializeInVelocityDirection = true;

        public bool surviveTerrainCollision = false;
        public bool surviveEnemyActorCollision = false;

        public GameObject spawnOnHit;

        private float timeAlive;

        private List<IDamagable> targetsDamaged = new List<IDamagable>();

        public string[] killMessages
        {
            get
            {
                var a = owner.GetComponent<BaseActor>();
                if (a == null)
                    return new string[] { " hit you in the head." };
                else
                    return a.killMessages;
            }
        }

        public static ProjectileBehaviour Fire(GameObject prefab, GameObject owner, Vector3 position, Vector3 direction, int damage)
        {
            var go = Instantiate(prefab, position, Quaternion.identity);

            var proj = go.GetComponent<ProjectileBehaviour>();
            if (proj.initializeInVelocityDirection)
                go.transform.right = direction;

            proj.damageDealt += damage;
            proj.owner = owner;
            var rb = proj.GetComponent<Rigidbody2D>();
            Vector3 dir = direction;

            if (proj.spread > 0f)
                dir = Quaternion.Euler(0f, 0f, (Engine.RandomFloat() - 0.5f) * proj.spread) * dir;

            Vector3 vel = dir * proj.startSpeed;

            rb.velocity = vel;
            rb.transform.position = new Vector3(rb.transform.position.x, rb.transform.position.y, position.z);
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0f);

            return proj;
        }

        private void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
            timeAlive += Time.deltaTime;
            if (timeAlive > lifeTime)
            {
                if (spawnOnHit != null)
                    Instantiate(spawnOnHit, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// returns true if the projectile should be destroyed
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnHitTarget(Collider2D collider)
        {
            var ownerActor = owner.GetComponent<BaseActor>();
            if (ownerActor != null)
            {
                var targetActor = collider.GetComponent<IDamagable>();
                var targetProjectile = collider.GetComponent<ProjectileBehaviour>();
                if (targetProjectile != null)
                    return false;

                if (targetActor != null)
                {
                    if (ownerActor.IsEnemy(targetActor.Team))
                    {
                        //Debug.Log(targetActor.Team + " is enemy to " + ownerActor.gameObject.name);
                        if (targetActor is ActorDamageRelay)
                        {
                            var rl = targetActor as ActorDamageRelay;
                            targetActor = rl.owningActor;
                        }

                        if (!targetsDamaged.Contains(targetActor))
                        {
                            targetActor.Damage(damageDealt, this);
                            targetsDamaged.Add(targetActor);
                        }
                        return !surviveEnemyActorCollision;
                    }
                    else
                    {
                        if ((IDamagable)ownerActor != targetActor)
                        {
                            if (targetActor is ActorDamageRelay)
                            {
                                if ((targetActor as ActorDamageRelay).owningActor != ownerActor)
                                    return false;
                            }
                            else
                                return false;

                        }
                    }
                    return false;
                }

            }

            return !surviveTerrainCollision;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {

            if (OnHitTarget(collision.collider))
            {
                if (spawnOnHit != null)
                    Instantiate(spawnOnHit, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }


        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (OnHitTarget(collision))
            {
                if (spawnOnHit != null)
                    Instantiate(spawnOnHit, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        public void OnKill(IDamagable target)
        {
            var ownerActor = owner.GetComponent<BaseActor>();
            if (ownerActor != null)
            {
                ownerActor.OnKill(target);
            }

        }
    }
}
