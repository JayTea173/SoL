using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoL.Actors;
using UnityEngine;

namespace SoL.Animation.FrameEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "Knockback Frame Event", menuName = "Animation/FrameEvents/Knockback")]
    public class KnockbackDamageTargets : FrameEventAsset
    {
        public AnimationCurve knockback;
        public float time = 0.5f;
        public float distanceMultiplier = 2f;

        public override void OnDamageTarget(SpriteAnimationBehaviour animated, SpriteFrame frame, IDamagable damaged)
        {
            base.OnDamageTarget(animated, frame, damaged);
            if (damaged is BaseActor)
                animated.StartCoroutine(KnockbackCoroutine(animated, damaged as BaseActor));
        }

        private IEnumerator KnockbackCoroutine(SpriteAnimationBehaviour user, BaseActor target)
        {
            float t = 0f;

            while (t < time)
            {
                var diff = (target.transform.position - user.transform.position);
                diff.z = 0f;
                target.transform.position += diff.normalized * knockback.Evaluate(t / time) * Time.deltaTime * distanceMultiplier;
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            yield return null;
        }
    }
}
