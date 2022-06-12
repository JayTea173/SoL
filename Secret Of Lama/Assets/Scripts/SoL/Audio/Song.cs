using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    [CreateAssetMenu(fileName = "Song", menuName = "Sound/Song")]
    public class Song : ScriptableObject
    {
        public AudioClip clip;
        public bool presistThroughDeathAnimation;

        [Serializable]
        public class Segment
        {
            public float secondsStart;
            public float secondsEnd;
            public int loopCount = 0;


            public void Play(AudioSource src)
            {
                float diff = src.time - secondsStart;
                if (Mathf.Abs(diff) > 0.25f)
                    src.time = secondsStart;
            }

            public bool Playing(AudioSource src)
            {
                return src.time >= secondsStart && src.time < secondsEnd;
            }
        }

        public void SetSegment(int targetSegment)
        {
            playbackCurrentSegment = targetSegment;
            segments[playbackCurrentSegment].Play(MusicPlayer.Instance.AudioSource);
        }



        private int playbackCurrentSegment;
        private int currentSegmentLooped;
        private int numSegments;

        public Segment[] segments;

        public void Play(AudioSource src)
        {
            src.clip = clip;
            src.Play();
            playbackCurrentSegment = 0;
            currentSegmentLooped = 0;
            numSegments = segments.Length;
            if (numSegments > 0)
            {
                segments[0].Play(src);
            }
        }

        public void UpdateSegmentedPlay(AudioSource src)
        {
            var seg = segments[playbackCurrentSegment];
            if (!seg.Playing(src))
            {

                int numLoopsLeft = seg.loopCount - currentSegmentLooped;
                if (numLoopsLeft <= 0 && seg.loopCount >= 0 || MusicPlayer.Instance.bypassNextSegmentLoop)
                {
                    MusicPlayer.Instance.bypassNextSegmentLoop = false;
                    currentSegmentLooped = 0;
                    playbackCurrentSegment++;
                    segments[playbackCurrentSegment].Play(src);
                }
                else
                {
                    seg.Play(src);
                    return;
                }
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                src.time += 5f;

            if (Input.GetKeyDown(KeyCode.Space))
                src.time = seg.secondsEnd - 1f;

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                src.time -= 5f;
                if (src.time < seg.secondsStart)
                {
                    src.time = seg.secondsEnd - (src.time - seg.secondsStart);
                }
            }
#endif
        }
    }
}
