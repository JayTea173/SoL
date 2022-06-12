using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoL.Actors;
using System.Collections;

namespace SoL.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ActorHUD : MonoBehaviour
    {
        [SerializeField]
        private Actors.CharacterActor actor;
        public TMP_Text hpDisplay;
        public TMP_Text levelDisplay;
        public Image attackGaugeImage;
        public RectTransform attackGaugeRect;
        public GameObject attackGauge;
        public Transform attackGaugeParent;
        public Image levelGauge;       
        public Image weaponLevelGauge;

        private float timeGaugeAtFull = 0f;

        private Color chargingTextColor;
        private CanvasGroup cg;
        public Color chargedTextColor;

        private void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            SetActor(actor, true);
            for (int i = 0; i < 10; i++)
            {
                var go = Instantiate(attackGauge, attackGaugeParent);
                go.transform.localScale = new Vector3(attackGaugeRect.localScale.x, 1.5f + (i / 2f), attackGaugeRect.localScale.z);
                go.GetComponent<Image>().color = new Color(1f-(i/3f), Mathf.Clamp(1f - ((i-3) / 3f),0f,1f), Mathf.Clamp(1f - ((i-6) / 3f),0f,1f));
            }
        }

        public void SetActor(Actors.CharacterActor actor, bool skipCallbackClear = false)
        {
            if (!skipCallbackClear)
            {
                this.actor.onDamageTaken -= OnDamageTaken;
                this.actor.onHealed -= OnDamageTaken;
            }
            this.actor = actor;
            actor.onDamageTaken += OnDamageTaken;
            actor.onHealed += OnDamageTaken;

            OnDamageTaken(0, null);
        }

        private int OnDamageTaken(int amount, IDamageSource damageSource)
        {

            StartCoroutine(DelayedUpdate());
            return amount;
        }

        private IEnumerator DelayedUpdate()
        {
            yield return new WaitForEndOfFrame();
            hpDisplay.text = actor.Hitpoints.ToString() + "/" + actor.HitpointsMax.ToString();
            
        }

        private void Update()
        {
            float chargeLevel = 0f;
            levelDisplay.text = actor.level.ToString();
            levelGauge.fillAmount = (actor.xp - actor.xpNext[actor.level - 1]) / (float)(actor.xpNext[actor.level] - actor.xpNext[actor.level - 1]);
            if (actor.weapon != null)
            {
                weaponLevelGauge.fillAmount = PlayerController.Instance.Actor.weapon.characterData[0].SkillProgress;
            }
            else
            {
                weaponLevelGauge.fillAmount = 0f;
            }           
            cg.alpha = actor.weapon == null ? 0f : 1f;
            if (actor.AttackCharge > 0f)
            {
                attackGaugeImage.fillAmount = actor.AttackCharge;
            }
            else
            {
                attackGaugeImage.fillAmount = 0f;
                attackGaugeImage.color = new Color(1,1,1);
            }          
            if (actor.AttackCharge >= 1f)
            {   
                chargeLevel = actor.AttackCharge-1;
                int c = attackGaugeParent.childCount;

                timeGaugeAtFull += Time.deltaTime;
                if (timeGaugeAtFull > 0.5f && !actor.Charging)
                {
                    attackGaugeImage.fillAmount = 0f;
                }
                for (int i = 0;i < c;i++)
                {
                    var I = attackGaugeParent.GetChild(i).GetComponent<Image>();
                    I.fillAmount = Mathf.Max(Mathf.Min(chargeLevel,1f),0);  
                    chargeLevel--;
                }

                    
                    //Debug.Log("attack Charge: " + actor.AttackCharge);                                             
                                                                                                      
            }
            else if (actor.Charging)
            {
                timeGaugeAtFull = 0f;                
            }
        }
    }
}
