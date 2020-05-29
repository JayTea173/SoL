using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace SoL.Animation
{
    [Serializable]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BaseActor))]
    public class SpriteAnimationBehaviour : MonoBehaviour
    {
        [Serializable]
        public class SpriteFrame
        {
            public Sprite sprite;
            public float durationMultiplier = 1f;

            public Flags flags;
            public Motion motion;
            public DamageFrame damage;

            public bool dealsDamage
            {
                get
                {
                    return damage.value >= 0f;
                }
            }

            [Serializable]
            public class Motion
            {
                public AnimationCurve motionX, motionY = AnimationCurve.Linear(0f, 0f, 0f, 0f);
                public float motionMultiplier = 16f;
                public bool invertMotionSamplingX, invertMotionSamplingY;
            }

            [Serializable]
            public class DamageFrame
            {
                public float value = -1f;
                [Tooltip("x positive is local forward")]
                public Vector2 originOffset = Vector2.right;
                public float radius = 1f;
            }

            [FlagsAttribute]
            public enum Flags
            {
                MOVEMENT_BLOCKED = 1 << 0,
                CHARGING_BLOCKED = 1 << 1,
                INVULNERABLE = 1 << 2
            }
        }


        [Serializable]
        public class SpriteAnimation
        {
            public string name;
            public List<SpriteFrame> frames;
            protected float totalDuration;
            public float Duration
            {
                get
                {
                    return totalDuration / fps;
                }
            }
            public float fps = 8f;

            public void CalculateDuration()
            {
                totalDuration = 0f;
                foreach (var frame in frames)
                {
                    totalDuration += frame.durationMultiplier;
                }
            }
        }

        public List<SpriteAnimation> animations;

        public int currentAnimationId = 0;
        private int currentAnimationFrameCount;
        private float currentAnimationFrameProgress = 0f;


        private float animationTime = 0f;
        private SpriteFrame currentFrame;

        public SpriteFrame CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }

        private SpriteRenderer sr;
        private BaseActor actor;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            actor = GetComponent<BaseActor>();
            foreach (var anim in animations)
                anim.CalculateDuration();
        }
        /// <summary>
        /// return true if animation changed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SetAnimation(int id, bool playFromStart = false)
        {
            if (id == currentAnimationId)
                return false;
            currentAnimationId = id;
            currentAnimationFrameCount = animations[id].frames.Count;
            if (playFromStart)
                animationTime = 0f;
            return true;
        }

        public SpriteAnimation CurrentAnimation
        {
            get
            {
                return animations[currentAnimationId];
            }
        }
        /// <summary>
        /// returns true if the animation was found
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playFromStart"></param>
        /// <returns></returns>
        public bool SetAnimation(string name, bool playFromStart = true)
        {
            int hc = name.GetHashCode();
            int index = animations.FindIndex((a) => a.name.GetHashCode() == hc);
            if (index < 0)
            {
                //Debug.LogError("Unable to find an animation called " + name + " on " + gameObject.name);
                return false;
            }

            targetsHitWithThisAnimation.Clear();

            SetAnimation(index, playFromStart);
            return true;

        }

        protected List<IDamagable> targetsHitWithThisAnimation = new List<IDamagable>();

        private void Update()
        {
            if (currentFrame != null)
            {
                float sampleTimeX = currentFrame.motion.invertMotionSamplingX ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;
                float sampleTimeY = currentFrame.motion.invertMotionSamplingY ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;

                transform.position += (Vector3)actor.TransformForwardX(new Vector2(currentFrame.motion.motionX.Evaluate(sampleTimeX), currentFrame.motion.motionY.Evaluate(sampleTimeY))) * currentFrame.motion.motionMultiplier * Time.deltaTime / Engine.pixelsPerUnit;

                if (currentFrame.dealsDamage)
                {
                    Vector2 offset = actor.TransformDirection(currentFrame.damage.originOffset);
                    var hits = Engine.QuadTree.GetAgentsInRange(transform.position + actor.PhysicsAgent.b.center + (Vector3)offset, currentFrame.damage.radius).Where((h) =>
                    {
                        if (h.t == transform)
                            return false;
                        var a = h.t.GetComponent<IDamagable>();
                        return actor.IsEnemy(a.Team);
                    });
                    foreach (var hit in hits)
                    {
                        var damageable = hit.t.GetComponent<IDamagable>();

                        if (!targetsHitWithThisAnimation.Contains(damageable))
                        {
                            targetsHitWithThisAnimation.Add(damageable);
                            damageable.Damage(Mathf.FloorToInt(currentFrame.damage.value * actor.GetDamageDealt()), actor);
                        }
                    }

                }

            }
        }

        public SpriteFrame.Flags GetCurrentFrameFlags()
        {
            if (currentFrame == null)
                return default(SpriteFrame.Flags);
            return currentFrame.flags;
        }

        public void Advance(float seconds)
        {

            animationTime += seconds;
            if (currentAnimationId < 0 || currentAnimationFrameCount == 0)
                return;

            int frame = 0;
            var anim = animations[currentAnimationId];
            float d = anim.Duration;
            float t = animationTime % d;
            float frameDuration = 0f;

            while ((frameDuration = anim.frames[frame].durationMultiplier / anim.fps) < t)
            {
                t -= frameDuration;
                frame++;
                if (frame >= currentAnimationFrameCount)
                    frame %= currentAnimationFrameCount;


            }

            currentFrame = anim.frames[frame];
            sr.sprite = currentFrame.sprite;

            float frameTime = currentFrame.durationMultiplier / anim.fps;
            float prog = t / frameTime;
            currentAnimationFrameProgress = prog;
        }
    }
}
