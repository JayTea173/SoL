using SoL.Actors;
using SoL.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation.FrameEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Frame Event", menuName = "Animation/FrameEvents/Projectile")]
    public class SpawnProjectileFrameEvent : FrameEventAsset
    {
        public GameObject prefab;

        public int count = 1;
        public float[] timeouts;
        public AudioClip soundOnFirstProjectile;
        public Vector2 offset;



        public override void OnFrameEnter(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {

            animated.StartCoroutine(SpawnProjectilesCoroutine(animated, frame, prefab.GetComponent<HomingProjectileBehaviour>() != null));

        }


        private IEnumerator SpawnProjectilesCoroutine(SpriteAnimationBehaviour animated, SpriteFrame frame, bool isHoming)
        {
            BaseActor actor = animated.GetComponent<BaseActor>();
            int c = timeouts != null ? timeouts.Length : 0;
            for (int i = 0; i < count; i++)
            {
                float timeout = 0f;

                if (i == 0)
                    if (soundOnFirstProjectile != null)
                    {
                        var src = animated.GetComponent<AudioSource>();
                        if (src != null)
                        {
                            src.pitch = Engine.RandomFloat(0.9f, 1.1f);
                            src.PlayOneShot(soundOnFirstProjectile);
                            src.pitch = 1.0f;

                        }
                    }

                if (c != 0)
                    timeout = timeouts[i % c];

                bool isActor = prefab.GetComponent<BaseActor>();


                if (timeout > 0f)
                    yield return new WaitForSeconds(timeout);

                Vector3 pos = animated.transform.position + (Vector3)actor.TransformDirection(offset);

                if (isActor)
                {
                    var go = Instantiate(prefab, pos, Quaternion.identity);
                    var ac = go.GetComponent<BaseActor>();
                    ac.Move(actor.movementDirection);
                    ac.transform.right = Vector3.right;
                } else if (isHoming)
                    HomingProjectileBehaviour.Fire(prefab, animated.gameObject, pos, actor.movementDirection, Mathf.FloorToInt(frame.damage.value * actor.GetDamageDealt()), animated.GetComponent<AI.DefaultEnemyAI>().Target.transform);
                else
                    ProjectileBehaviour.Fire(prefab, animated.gameObject, pos, actor.movementDirection, Mathf.FloorToInt(frame.damage.value * actor.GetDamageDealt()));
            }
        }
    }
}
