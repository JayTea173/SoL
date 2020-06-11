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
    public class FrameEventAsset : ScriptableObject
    {
        public virtual void OnFrameEnter(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {

        }
        public virtual void OnFrameExit(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {

        }
        
        public virtual void OnDamageTarget(SpriteAnimationBehaviour animated, SpriteFrame frame, IDamagable damaged)
        {

        }
    }
}
