using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BlackMask : MonoBehaviour
    {
        private static BlackMask instance;
        public static BlackMask Instance
        {
            get
            {
                return instance;
            }
        }

        private CanvasGroup cg;

        private void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            instance = this;
        }

        public void FadeOut(float seconds)
        {
            StartCoroutine(FadeOutCoroutine(seconds));
        }

        public void FadeIn(float seconds)
        {
            StartCoroutine(FadeInCoroutine(seconds));
        }

        public IEnumerator FadeOutCoroutine(float seconds)
        {
            cg.alpha = 0f;
            float t = 0f;
            while (t < seconds)
            {

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                cg.alpha = t / seconds;
            }

            cg.alpha = 1f;
        }

        public IEnumerator FadeInCoroutine(float seconds)
        {
            cg.alpha = 1f;
            float t = 0f;
            while (t < seconds)
            {

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                cg.alpha = 1f - t / seconds;
            }

            cg.alpha = 0f;
        }
    }
}
