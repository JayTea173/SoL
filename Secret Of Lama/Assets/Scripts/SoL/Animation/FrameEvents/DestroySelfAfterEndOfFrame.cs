using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation.FrameEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "Destroy Self Frame Event", menuName = "Animation/FrameEvents/Destroy Self")]
    public class DestroySelfAfterEndOfFrame : FrameEventAsset
    {
        public override void OnFrameExit(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {
            base.OnFrameExit(animated, frame);
            Destroy(animated.gameObject);
        }
    }
}
