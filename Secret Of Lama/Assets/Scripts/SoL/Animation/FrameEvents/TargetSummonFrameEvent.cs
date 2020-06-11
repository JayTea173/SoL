using SoL.Actors;
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
    [CreateAssetMenu(fileName = "Summon Frame Event", menuName = "Animation/FrameEvents/Summon")]
    public class TargetSummonFrameEvent : FrameEventAsset
    {
        public GameObject[] summonPrefabs;

        public int count;
        public float[] timeouts;

        public float spread = 0f;
        public Vector3 offset = Vector3.zero;


        public override void OnFrameEnter(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {
            base.OnFrameEnter(animated, frame);
            var ai = animated.GetComponent<AI.DefaultEnemyAI>();
            if (ai == null)
                return;

            if (ai.Target == null)
                return;

            animated.StartCoroutine(SpawnSummonsCoroutine(ai.Target.transform, animated, frame));

        }

        private IEnumerator SpawnSummonsCoroutine(Transform target, SpriteAnimationBehaviour animated, SpriteFrame frame)
        {
            int numPrefabs = summonPrefabs.Length;
            int c = timeouts != null ? timeouts.Length : 0;
            
            for (int i = 0; i < count; i++)
            {
                float timeout = 0f;


                if (c != 0)
                    timeout = timeouts[i % c];

                if (timeout > 0f)
                    yield return new WaitForSeconds(timeout);

                var pre = summonPrefabs[Engine.RandomInt(0, numPrefabs)];
               
                Vector3 p = target.position + offset + new Vector3(Engine.RandomFloat(-1f, 1f), Engine.RandomFloat(-1f, 1f), 0f) * spread;
                var go = Instantiate(pre, p, Quaternion.identity);
                go.GetComponent<BaseActor>().position = p;
                go.transform.position = p;
                Debug.Log("Spawn @" + p);
            }
            
        }
    }
}
