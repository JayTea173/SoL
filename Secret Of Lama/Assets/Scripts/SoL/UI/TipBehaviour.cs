using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace SoL
{
    [RequireComponent(typeof(CanvasGroup), typeof(TMP_Text))]
    public class TipBehaviour : MonoBehaviour
    {
        private CanvasGroup cg;
        private TMP_Text textField;

        private static TipBehaviour instance;
        public static TipBehaviour Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            textField = GetComponent<TMP_Text>();
            cg.blocksRaycasts = false;
            cg.alpha = 0f;
            instance = this;
        }

        public void SetVisibility(bool visible)
        {
            cg.alpha = visible ? 1f : 0f;
        }

        public void SetText(string text)
        {
            textField.text = text;
        }

    }
}
