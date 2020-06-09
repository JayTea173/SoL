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

        void OnTriggerEnter2D(Collider2D col)
        {
            Engine.Instance.levelManager.Load(nextSceneName, true);
        }

       
    }
}
