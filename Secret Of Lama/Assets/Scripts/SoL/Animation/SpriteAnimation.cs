using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation
{
    [Serializable]
    public class SpriteAnimation
    {
        public string name;
        public List<SpriteFrame> frames;

        public Mirroring[] mirroring;

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
            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    totalDuration += frame.durationMultiplier;
                }
            }
        }

        private void OnEnable()
        {
            CalculateDuration();
        }

        public SpriteAnimation()
        {
            mirroring = new Mirroring[4];
            for (int i = 0; i < 4; i++)
                mirroring[i] = new Mirroring(false, false);
        }

        public SpriteAnimation Copy()
        {
            SpriteAnimation copy = new SpriteAnimation();
            copy.name = name;
            copy.frames = new List<SpriteFrame>();
            foreach (var f in frames)
                copy.frames.Add(f);
            for (int i = 0; i < 4; i++)
                copy.mirroring[i].value = mirroring[i].value;

            return copy;
        }
    }
}
