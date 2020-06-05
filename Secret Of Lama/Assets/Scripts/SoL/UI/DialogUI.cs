using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Collections;

namespace SoL.UI
{
    public class DialogUI : MonoBehaviour
    {
        public TMP_Text textField;
        private CanvasGroup cg;
        private Vector3 originalSize;

        private void Awake()
        {
            originalSize = transform.localScale;
            cg = GetComponent<CanvasGroup>();
            SetVisibility(false, false);

        }

        private void SetVisibility(bool visible, bool animate = true)
        {
            cg.blocksRaycasts = visible;
            cg.alpha = visible ? 1f : (animate ? 1f : 0f);
            if (animate)
                StartCoroutine(visible ? PlayShowAnimation() : PlayHideAnimation());
        }

        private IEnumerator PlayShowAnimation()
        {
            transform.localScale = new Vector3(0f, originalSize.y, originalSize.z);
            float t = 0f;
            while (t < 0.66f)
            {
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                transform.localScale = new Vector3(t / 0.66f * originalSize.x, originalSize.y, originalSize.z);
            }
            transform.localScale = originalSize;
            yield return null;
        }

        private IEnumerator PlayHideAnimation()
        {
            transform.localScale = originalSize;
            float t = 0f;
            while (t < 0.66f)
            {
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                transform.localScale = new Vector3(1f - (t / 0.66f) * originalSize.x, originalSize.y, originalSize.z);
            }
            transform.localScale = new Vector3(0f, originalSize.y, originalSize.z);
            yield return null;
        }
    }
}
