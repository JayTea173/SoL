using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController instance;
        public static CameraController Instance
        {
            get
            {
                return instance;
            }
        }

        private int shake = 0;
        private bool shaking = false;
        private Vector3 shakeOffset = Vector3.zero;

        public void Shake(int pixels)
        {
            shake = pixels;
            if (pixels > 0)
                shaking = true;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (shaking)
            {
                if (Time.frameCount % 2 == 0)
                {
                    transform.position -= shakeOffset;
                    shakeOffset = new Vector3(0f, UnityEngine.Random.Range(0.9f, 1f), 0f);
                    if (Time.frameCount % 4 == 0)
                    {
                        shake--;
                        shakeOffset = -shakeOffset;
                    }

                    shakeOffset *= (shake / (float)Engine.pixelsPerUnit);
                    transform.position += shakeOffset;
                    if (shake < 0)
                    {
                        shake = 0;
                        shaking = false;
                    }
                }
            }
        }
    }
}
