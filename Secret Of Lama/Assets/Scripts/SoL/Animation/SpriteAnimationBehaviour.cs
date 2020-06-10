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
        private AnimationCollection currentCollection = null;

        public float neutralHeight = 0f;


        private float animationTime = 0f;
        private SpriteFrame currentFrame;

        public SpriteFrame CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }

        public SpriteRenderer spriteRenderer;
        private BaseActor actor;

        private void Awake()
        {
           
            actor = GetComponent<BaseActor>();

            neutralHeight = spriteRenderer.transform.localPosition.y;
            actor.flyingHeight = neutralHeight;

            foreach (SpriteAnimation anim in animations)
                anim.CalculateDuration();

            currentCollection = animations;

        }

        private bool frameChanged = false;

        private void FixedUpdate()
        {
            if (currentFrame != null)
            {
                float sampleTimeX = currentFrame.motion.invertMotionSamplingX ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;
                float sampleTimeY = currentFrame.motion.invertMotionSamplingY ? (1f - currentAnimationFrameProgress) : currentAnimationFrameProgress;

                float rootMotionMultiplier = currentFrame.motion.motionMultiplier * Time.fixedDeltaTime / Engine.pixelsPerUnit;

                Vector3 delta = (Vector3)actor.TransformForwardX(new Vector2(currentFrame.motion.motionX.Evaluate(sampleTimeX), 0f));

                Vector3 targetPosition = transform.position + delta * rootMotionMultiplier;

                float flyDelta = currentFrame.motion.motionY.Evaluate(sampleTimeY) * rootMotionMultiplier;
                if (flyDelta == 0f)
                    actor.flyingHeight = Mathf.MoveTowards(actor.flyingHeight, neutralHeight, Time.fixedDeltaTime * 16f);

                actor.flyingHeight += flyDelta;
                spriteRenderer.transform.localPosition = new Vector3(0f, actor.flyingHeight, 0f);


                actor.MoveToCheckingCollision(targetPosition);


                
                if (currentFrame.dealsDamage)
                {
                    actor.HandleDamageFrame(currentFrame, targetsHitWithThisAnimation, frameChanged);
                }
                frameChanged = false;
            }
        }

        private void Update()
        {

            

        }

        /// <summary>
        /// return true if animation changed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SetAnimation(int id, bool playFromStart = false)
        {
            return SetAnimation(animations, id, playFromStart);
        }

        public bool SetAnimation(AnimationCollection animations, int id, bool playFromStart = false)
        {
            if (id == currentAnimationId && currentCollection == animations)
                return false;
            currentAnimationId = id;
            currentCollection = animations;
            currentAnimationFrameCount = currentCollection[id].frames.Count;
            if (playFromStart)
                animationTime = 0f;

            Advance(0f);
            return true;
        }



        public SpriteAnimation CurrentAnimation
        {
            get
            {
                return currentCollection[currentAnimationId];
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
            int index = currentCollection.GetIndexByName(name);
            if (index < 0)
            {
                //Debug.LogError("Unable to find an animation called " + name + " on " + gameObject.name);
                return false;
            }

            targetsHitWithThisAnimation.Clear();
            SetAnimation(index, playFromStart);
            return true;

        }

        public bool SetAnimation(AnimationCollection animations, string name, bool playFromStart = true)
        {
            int hc = name.GetHashCode();
            int index = animations.GetIndexByName(name);
            if (index < 0)
            {
                Debug.LogError("Unable to find an animation called " + name + " in " + animations.name);
                return false;
            }

            targetsHitWithThisAnimation.Clear();
            SetAnimation(animations, index, playFromStart);
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
            var anim = currentCollection[currentAnimationId];
            float d = anim.Duration;
            if (d <= 0f)
            {
                anim.CalculateDuration();
                d = anim.Duration;
            }
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
            if (spriteRenderer.sprite != s)
            {
                spriteRenderer.sprite = s;
                frameChanged = true;
                if (currentFrame.flags.HasFlag(FrameFlags.SWAP_FACING))
                {
                    actor.SetFacing(BaseActor.InvertFacing(actor.facing));
                    actor.Move(-actor.movementDirection);
                }
            }

            if (currentFrame.flags.HasFlag(FrameFlags.IGNORE_ACTOR_COLLISIONS))
            {
                if (actor.gameObject.layer == Engine.actorLayer)
                    actor.gameObject.layer = Engine.airborneActorLayer;

            }
            else if (actor.gameObject.layer == Engine.airborneActorLayer)
                actor.gameObject.layer = Engine.actorLayer;

            float frameTime = currentFrame.durationMultiplier / anim.fps;
            float prog = t / frameTime;
            currentAnimationFrameProgress = prog;
        }
    }
}
