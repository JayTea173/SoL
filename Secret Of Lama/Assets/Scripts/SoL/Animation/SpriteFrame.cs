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
    public class SpriteFrame
    {
        [Serializable]
        public class SpriteData
        {
            public Sprite sprite;

            public static implicit operator Sprite(SpriteData other)
            {
                return other.sprite;
            }

            public SpriteData()
            {
            }
        }

        public SpriteData left, right, down, up;

        
        public SpriteFrame()
        {
            left = new SpriteData();
            right = new SpriteData();
            down = new SpriteData();
            up = new SpriteData();
        }
        
        public float durationMultiplier = 1f;

        public FrameFlags flags;
        public Motion motion;
        public DamageFrame damage;

        public bool dealsDamage
        {
            get
            {
                return damage.value >= 0f;
            }
        }

        public SpriteData this[BaseActor.Facing facing]
        {
            get
            {
                switch (facing)
                {
                    case BaseActor.Facing.Left:
                        return left;
                    case BaseActor.Facing.Right:
                        return right;
                    case BaseActor.Facing.Down:
                        return down;
                    case BaseActor.Facing.Up:
                        return up;
                }
                return null;
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
            [Tooltip("x positive is local forward, y positive is local up")]
            public Vector2 originOffset = Vector2.right;
            public float radius = 1f;
        }


    }

    [FlagsAttribute]
    public enum FrameFlags
    {
        MOVEMENT_BLOCKED = 1 << 0,
        CHARGING_BLOCKED = 1 << 1,
        INVULNERABLE = 1 << 2,
        SWAP_FACING = 1 << 3,
        IGNORE_ACTOR_COLLISIONS = 1 << 4
    }

    [Serializable]
    public class Mirroring
    {
        public byte value;

        public Mirroring(bool x, bool y)
        {
            value = (byte)((x ? 1 : 0) + (y ? 2 : 0));
        }

        public bool X
        {
            get
            {
                return (value & 1) != 0;
            }
        }

        public bool Y
        {
            get
            {
                return (value & 2) != 0;
            }
        }

        public void Apply(SpriteRenderer sr)
        {
            switch (value)
            {
                case 0:
                    sr.flipX = false;
                    sr.flipY = false;
                    break;
                case 1:
                    sr.flipX = true;
                    sr.flipY = false;
                    break;
                case 2:
                    sr.flipX = false;
                    sr.flipY = true;
                    break;
                case 3:
                    sr.flipX = true;
                    sr.flipY = true;
                    break;
            }
        }
    }
}
