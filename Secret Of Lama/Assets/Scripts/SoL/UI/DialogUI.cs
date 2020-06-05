using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Collections;
using SoL.Actors;

namespace SoL.UI
{
    public class DialogUI : MonoBehaviour
    {
        public TMP_Text textField;
        [Range(4f, 100f)]
        public float charsPerSecond = 20f;
        public AudioSource audioSource;
        
        private CanvasGroup cg;
        private Vector3 originalSize;

        private int currentPageId = 0;
        private Dialog dialog;
        private Dialog.Page currentPage;
        private bool animatingPage = false;
        private int pagePosition = 0;
        private DialogPartner initiator, initiated;
        private DialogPartner currentSpeaking;

        private static DialogUI instance;
        public static DialogUI Instance
        {
            get
            {
                return instance;
            }
        }

        public bool visible
        {
            get
            {
                return cg.blocksRaycasts;
            }
            set
            {
                SetVisibility(value);
            }
        }

        private void Awake()
        {
            instance = this;
            originalSize = transform.localScale;
            cg = GetComponent<CanvasGroup>();
            SetVisibility(false, false);
           
        }

        public void Display(Dialog dialog, DialogPartner initiator, DialogPartner initiated, int startingPage = 0)
        {
            if (animatingOpenClose)
                return;
            this.dialog = dialog;
            SetVisibility(true);
            this.initiated = initiated;
            this.initiator = initiator;
            DisplayPage(startingPage);
            
        }

        private bool skip = false;

        public void OnNextDown()
        {
            skip = true;
            if (!animatingPage)
                NextPage();
        }

        public void OnNextUp()
        {
            skip = false;
        }

        protected void NextPage()
        {
            currentPageId++;
            DisplayPage(currentPageId);
        }

        protected void Close()
        {
            SetVisibility(false);
        }

        protected void DisplayPage(int pageId)
        {
            if (pageId >= dialog.pages.Count)
                Close();
            currentPageId = pageId;
            currentPage = dialog.pages[pageId];
            textField.text = string.Empty;
            animatingPage = true;
            pagePosition = 0;
            currentSpeaking = currentPage.delivererId == 0 ? initiated : initiator;
            StartCoroutine(AnimateTextCoroutine());
        }

        private int updates;
        private void Update()
        {
            if (visible)
            {
                updates++;
                if (updates > 10)
                {
                    updates -= 10;
                    if (Engine.RandomFloat() < 0.2f)
                    {
                        initiated.GetComponent<BaseActor>().LookAt(initiator.transform.position);
                    }
                }
            }
        }


        private IEnumerator AnimateTextCoroutine()
        {
            while (animatingOpenClose)
                yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(0.2f);
            if (animatingPage)
            {
                int l = currentPage.text.Length;
                float timePerChar = (1f / charsPerSecond);

                while (pagePosition < l)
                {
                    char c = currentPage.text[pagePosition];
                    pagePosition++;
                    float waitMultiplier = 1f;
                    switch (c)
                    {
                        case 'a':
                        case 'e':
                        case 'i':
                        case 'o':
                        case 'u':
                            waitMultiplier = 2f;
                            break;
                        case ' ':
                            waitMultiplier = 3f;
                            break;
                        case ',':
                            waitMultiplier = 8f;
                            break;
                        case '.':
                        case '!':
                        case '?':
                            waitMultiplier = 12f;
                            break;
                        default:
                            audioSource.pitch = (float)Engine.Gaussian(1.0d, 0.01d) + Mathf.Sin(Time.time * 0.7f) * 0.05f;
                            audioSource.PlayOneShot(currentSpeaking.voice, Engine.RandomFloat(0.8f, 1.0f));

                            break;

                    }
                    if (skip)
                        waitMultiplier *= 0.5f;
                    int chars = System.Math.Min(pagePosition, l);
                    if (chars >= l)
                        animatingPage = false;

                    textField.text = currentSpeaking.gameObject.name + ": " + currentPage.text.Insert(chars, "<color=#00000000>") + "</color>";
                    yield return new WaitForSeconds(1f / charsPerSecond * waitMultiplier);
                }


            }
            yield return null;
        }

        private void SetVisibility(bool visible, bool animate = true)
        {
            cg.blocksRaycasts = visible;
            cg.alpha = visible ? 1f : (animate ? 1f : 0f);
            if (animate)
                StartCoroutine(visible ? PlayShowAnimation() : PlayHideAnimation());
        }

        private bool animatingOpenClose = false;

        private IEnumerator PlayShowAnimation()
        {
            animatingOpenClose = true;
            transform.localScale = new Vector3(0f, originalSize.y, originalSize.z);
            float t = 0f;
            while (t < 0.33f)
            {
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                transform.localScale = new Vector3(t / 0.33f * originalSize.x, originalSize.y, originalSize.z);
            }
            transform.localScale = originalSize;
            animatingOpenClose = false;
            yield return null;
        }

        private IEnumerator PlayHideAnimation()
        {
            animatingOpenClose = true;
            transform.localScale = originalSize;
            float t = 0f;
            while (t < 0.33f)
            {
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                transform.localScale = new Vector3((1f - t / 0.33f) * originalSize.x, originalSize.y, originalSize.z);
            }
            transform.localScale = new Vector3(0f, originalSize.y, originalSize.z);
            yield return null;
            animatingOpenClose = false;
        }
    }
}
