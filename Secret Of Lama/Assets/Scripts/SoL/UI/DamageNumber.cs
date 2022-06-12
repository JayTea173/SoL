using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoL.UI
{
    public class DamageNumber : MonoBehaviour
    {
        public TMP_Text[] digits;

        [SerializeField]
        private TMP_Text digitPrefab;

        private float t = 0f;
        public float padding = 8f;

        private Transform target;

        private float baseSize;
        

        public float FontSize
        {
            get
            {
                return digits[0].fontSize;
            }
            set
            {
                int c = digits.Length;
                for (int i = 0; i < c; i++)
                {
                    digits[i].fontSize = value;
                }
            }
        }

        public Color Color
        {
            get
            {
                return digits[0].color;
            }
            set
            {
                int c = digits.Length;
                for (int i = 0; i < c; i++)
                {
                    digits[i].color = value;
                }
            }
        }



        private void Awake()
        {
            digitPrefab.gameObject.SetActive(false);
            baseSize = digitPrefab.fontSize;
        }

        private void UpdatePosition()
        {
            if (transform == null || target == null)
                Destroy(gameObject);
            else if (transform != null) 
                transform.position = Camera.main.WorldToScreenPoint(target.position);
        }

        private void Update()
        {
            t += Time.deltaTime;

            UpdatePosition();

            if (t > 3f)
            {
                float t0 = t - 3f;
                foreach (var d in digits)
                {
                    Color c = digitPrefab.color * (1f - t0);
                    c.a = digitPrefab.color.a;
                    d.color = c;
                }
                if (t >= 3.5f)
                {
                    float t1 = 1f - (t - 3.5f) * 2f;
                    int c = digits.Length;
                    for (int i = 1; i < c; i++)
                    {
                        digits[i].transform.position = Engine.AlignToPixelGrid(digits[0].transform.position + Vector3.right * padding * t1);
                    }
                }
                if (t >= 4f)
                    Destroy(gameObject);
            } 
        }

        private void SetText(string text)
        {
            if (digits != null)
            {
                int c0 = digits.Length;
                for (int i = 0; i < c0; i++)
                {
                    Destroy(digits[i].gameObject);
                }
            }
            int c = text.Length;

            digits = new TMP_Text[c];
            for (int i = 0; i < c; i++)
            {
                digits[i] = Instantiate(digitPrefab.gameObject, transform).GetComponent<TMP_Text>();
                digits[i].SetText(new String(text[i], 1));
                if (i > 0)
                    digits[i].transform.position = digits[i - 1].transform.position + Vector3.right * padding;
                digits[i].gameObject.SetActive(true);
            }
        }

        public static void Display(Transform parent, int amount)
        {
            var go = GameObject.Instantiate(amount < 50 ? StaticData.Instance.damageNumberPrefab : StaticData.Instance.damageNumberPrefabLarge, Engine.MainCanvas.transform);
            var dn = go.GetComponent<DamageNumber>();
            dn.target = parent;
            dn.SetText(amount.ToString());
            dn.UpdatePosition();
        }

        public static void Display(Transform parent, string text)
        {
            var go = GameObject.Instantiate(StaticData.Instance.damageNumberPrefab, Engine.MainCanvas.transform);
            var dn = go.GetComponent<DamageNumber>();
            dn.target = parent;
            dn.Color = Color.green;
            dn.SetText(text);
            dn.FontSize *= .66f;
            dn.padding *= .66f;
            dn.UpdatePosition();
        }
    }
}
