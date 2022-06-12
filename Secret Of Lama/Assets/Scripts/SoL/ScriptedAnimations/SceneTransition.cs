using SoL.Actors;
using SoL.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.ScriptedAnimations
{
    public class SceneTransition : MonoBehaviour
    {
        public string nextSceneName;
        public bool stopMusic;

        void OnTriggerEnter2D(Collider2D col)
        {
            var d = col.GetComponent<IDamagable>();
            BaseActor a = null;
            if (d is ActorDamageRelay)
            {
                a = (d as ActorDamageRelay).owningActor;
            }
            else if (d is BaseActor)
            {
                a = d as BaseActor;
            }
            if (a != null)
            {

                Engine.Instance.levelManager.Load(nextSceneName, true);
                if (stopMusic)
                    MusicPlayer.Instance.Stop();
            }
        }
    }

}
