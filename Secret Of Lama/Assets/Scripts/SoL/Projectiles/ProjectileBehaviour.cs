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

        private float timeAlive;

        public static ProjectileBehaviour Fire(GameObject prefab, GameObject owner, Vector3 position, Vector3 direction, int damage)
        {
            var go = Instantiate(prefab, position, Quaternion.identity);

            var proj = go.GetComponent<ProjectileBehaviour>();
            if (proj.initializeInVelocityDirection)
                go.transform.right = direction;

            Debug.Log("Initial damage for proj: " + damage);
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
                Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var ownerActor = owner.GetComponent<BaseActor>();
            if (ownerActor != null)
            {
                var targetActor = collision.collider.GetComponent<IDamagable>();
                if (targetActor != null)
                {
                    Debug.Log("Hit " + collision.collider.gameObject.name);
                    if (ownerActor.IsEnemy(targetActor.Team))
                    {
                        targetActor.Damage(damageDealt, this);

                    }
                    else if ((IDamagable)ownerActor != targetActor)
                    {
                        if (targetActor is ActorDamageRelay)
                        {
                            if ((targetActor as ActorDamageRelay).owningActor != ownerActor)
                                Destroy(gameObject);
                        }
                       
                    }

                }

            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var ownerActor = owner.GetComponent<BaseActor>();
            if (ownerActor != null)
            {
                var targetActor = collision.GetComponent<IDamagable>();
                if (targetActor != null)
                {
                    if (ownerActor.IsEnemy(targetActor.Team))
                    {
                        targetActor.Damage(damageDealt, this);
                        Destroy(gameObject);
                    }

                }

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
