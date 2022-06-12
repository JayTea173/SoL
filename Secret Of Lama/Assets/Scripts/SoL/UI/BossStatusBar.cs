using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoL.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BossStatusBar : MonoBehaviour
    {
        private CanvasGroup cg;
        public Image fillImg;
        public TMP_Text nameField;


        protected CreatureActor boss;

        private static BossStatusBar instance;
        public static BossStatusBar Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
            cg = GetComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        public void SetBoss(CreatureActor actor)
        {
            this.boss = actor;
            nameField.text = actor.GetComponent<DialogPartner>().displayName;

        }

        private void Update()
        {

            float alpha = boss == null ? 0f : 1f;
            cg.alpha = Mathf.Lerp(cg.alpha, alpha, Time.deltaTime * 4f);
            if (boss != null)
                fillImg.fillAmount = Mathf.Lerp(fillImg.fillAmount, boss.GetHPPercent(), Time.deltaTime * 8f);
        }
    }
}
