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


        public AnimationCollection animations;

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

            foreach (SpriteAnimation anim in animations)
                anim.CalculateDuration();

        }


        private void Update()
        {

            if (currentFrame != null)
            {
                float sampleTimeX = currentFrame.motion.invertMotionSamplingX ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;
                float sampleTimeY = currentFrame.motion.invertMotionSamplingY ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;

                Vector3 delta = (Vector3)actor.TransformForwardX(new Vector2(currentFrame.motion.motionX.Evaluate(sampleTimeX), currentFrame.motion.motionY.Evaluate(sampleTimeY))) * currentFrame.motion.motionMultiplier * Time.deltaTime / Engine.pixelsPerUnit;

                Vector3 targetPosition = transform.position + delta;

                actor.MoveToCheckingCollision(targetPosition);

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

            Advance(0f);
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
            int index = animations.GetIndexByName(name);
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

        public static int GetFrameIndex(SpriteAnimation anim, float animationTime, out float t)
        {

            int frame = 0;
            float d = anim.Duration;
            t = animationTime % d;
            float frameDuration = 0f;

            int currentAnimationFrameCount = anim.frames.Count;

            while ((frameDuration = anim.frames[frame].durationMultiplier / anim.fps) < t)
            {
                t -= frameDuration;
                frame++;
                if (frame >= currentAnimationFrameCount)
                    frame %= currentAnimationFrameCount;


            }

            return frame;
        }

        public static int GetFrameIndex(SpriteAnimation anim, float animationTime)
        {

            float t = 0f;
            return GetFrameIndex(anim, animationTime, out t);
        }


        public static Vector2 FacingToVector(Actors.BaseActor.Facing facing)
        {
            switch (facing)
            {
                case BaseActor.Facing.Left:
                    return Vector2.left;
                case BaseActor.Facing.Right:
                    return Vector2.right;
                case BaseActor.Facing.Down:
                    return Vector2.down;
                case BaseActor.Facing.Up:
                    return Vector2.up;
                default:
                    return Vector2.right;
            }
        }


        public static Vector2 SampleMotion(SpriteAnimation anim, float animationTime)
        {
            float t = 0f;
            int frame = GetFrameIndex(anim, animationTime, out t);


            var currentFrame = anim.frames[frame];

            float frameTime = currentFrame.durationMultiplier / anim.fps;
            float prog = t / frameTime;
            float currentAnimationFrameProgress = prog;

            float sampleTimeX = currentFrame.motion.invertMotionSamplingX ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;
            float sampleTimeY = currentFrame.motion.invertMotionSamplingY ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;
            return new Vector2(currentFrame.motion.motionX.Evaluate(sampleTimeX), currentFrame.motion.motionY.Evaluate(sampleTimeY));
        }

        public static Vector2 Move(Vector2 motion, Vector2 movementDirection)
        {
            Vector2 v = motion;
            v.x *= movementDirection.x;
            v.y += movementDirection.y * motion.x;
            return v / Engine.pixelsPerUnit;
        }


        public FrameFlags GetCurrentFrameFlags()
        {
            if (currentFrame == null)
                return default(FrameFlags);
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

            var s = currentFrame[actor.facing];
            if (sr.sprite != s)
                sr.sprite = s;

            float frameTime = currentFrame.durationMultiplier / anim.fps;
            float prog = t / frameTime;
            currentAnimationFrameProgress = prog;
        }
    }
}
