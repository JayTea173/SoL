using SoL.Actors;
using SoL.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SoL.UI.Dialog;

namespace SoL.Animation.FrameEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "Dialog Action Frame Event", menuName = "Animation/FrameEvents/DialogAction")]
    public class DialogActionFrameEvent : FrameEventAsset
    {
        public List<DialogAction> actions;

        public override void OnFrameEnter(SpriteAnimationBehaviour animated, SpriteFrame frame)
        {
            foreach (var action in actions)
            {
                //Debug.Log("Frame running " + action.action.ToString());
                action.Run(animated, animated.GetComponent<DialogPartner>(), null);
            }
        }

    }
}
