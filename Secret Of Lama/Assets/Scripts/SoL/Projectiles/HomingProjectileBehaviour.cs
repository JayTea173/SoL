using SoL.Audio;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class HomingProjectileBehaviour : ProjectileBehaviour
    {
        public Transform target;
        public float turnRate = 10f;
        public float thrust = 10f;

        public Soundbank launchSound;
        public AudioClip projectileLoop;
        public Soundbank explosionSound;

        private Rigidbody2D rb;
        private AudioSource audioSource;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (target != null)
            {
                Debug.Log("ROT!");
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), Time.deltaTime * turnRate);
                Vector3 diff = (target.transform.position - transform.position);
                diff.z = 0f;
                rb.AddForce(diff.normalized * thrust * Time.deltaTime);
                transform.right = rb.velocity.normalized;

            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            if (explosionSound != null)
                AudioSource.PlayClipAtPoint(explosionSound.GetRandom(), transform.position);
        }

        public static HomingProjectileBehaviour Fire(GameObject prefab, GameObject owner, Vector3 position, Vector3 direction, int damage, Transform target)
        {
            var go = Instantiate(prefab, position, Quaternion.identity);
            go.transform.right = direction;
            var proj = go.GetComponent<HomingProjectileBehaviour>();
            proj.damageDealt += damage;
            proj.owner = owner;
            proj.target = target;
            var rb = proj.GetComponent<Rigidbody2D>();
            Vector3 dir = go.transform.right;

            if (proj.spread > 0f)
                dir = Quaternion.Euler(0f, 0f, (Engine.RandomFloat() - 0.5f) * proj.spread) * dir;

            Vector3 vel = dir * proj.startSpeed;

            rb.velocity = vel;

            if (proj.audioSource != null)
            {
                if (proj.launchSound != null)
                {
                    proj.audioSource.PlayOneShot(proj.launchSound.GetRandom());
                }
                if (proj.projectileLoop != null)
                {
                    proj.audioSource.clip = proj.projectileLoop;
                    proj.audioSource.Play();
                }
            }



            return proj;
        }
    }
}
