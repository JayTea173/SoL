using SoL.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation.FrameEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "Sound Frame Event", menuName = "Animation/FrameEvents/Sound")]
    public class PlaySoundFrameEvent : FrameEventAsset
    {
        public Soundbank soundBank;
        public float volume = 1f;

        public override void OnFrameEnter(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {
            base.OnFrameEnter(animated, frame);
            animated.GetComponent<AudioSource>().PlayOneShot(soundBank.GetRandom(), volume);
        }
    }
}
