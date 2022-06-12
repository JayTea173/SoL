using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.ScriptedAnimations
{
    public class CameraShake : MonoBehaviour
    {
        public int pixels;

        private void Start()
        {
            CameraController.Instance.Shake(pixels);
        }
    }
}
