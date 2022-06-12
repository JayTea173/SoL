using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation
{
    [Serializable]
    public class SingleAnimationLoop : MonoBehaviour
    {
        public float fps = 8f;
        public Frame[] frames;
        public SpriteRenderer sr;

        public bool playOnlyOnce;

        [Serializable]
        public class Frame
        {
            public Sprite sprite;
            public float timeMultiplier = 1f;
        }

        private float timeTotal = 0f;
        private float t = 0f;
        private int currFrame = -1;
        private float timePerFrame;

        private void Start()
        {
            timePerFrame = 1f / fps;
            foreach (var f in frames)
            {
                timeTotal += timePerFrame * f.timeMultiplier;
            }

            
        }

        private void Update()
        {
            t += Time.deltaTime;

            if (playOnlyOnce)
            {
                if (t > timeTotal)
                    Destroy(gameObject);
            }
            t %= timeTotal;


            float t0 = t;
            int i = -1;

            while (t0 > 0f)
            {
                t0 -= frames[++i].timeMultiplier * timePerFrame;
            }


            if (currFrame != i)
            {
                currFrame = i;

                
            }
            sr.sprite = frames[currFrame].sprite;      
        }
    }
}
