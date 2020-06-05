using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoL.Actors;

namespace SoL.UI
{
    public class ActorHUD : MonoBehaviour
    {
        [SerializeField]
        private Actors.BaseActor actor;
        public TMP_Text hpDisplay;
        public TMP_Text attackGauge;

        private float timeGaugeAtFull = 0f;

        private Color chargingTextColor;
        public Color chargedTextColor;

        private void Awake()
        {
            SetActor(actor, true);
            chargingTextColor = attackGauge.color;
        }

        public void SetActor(Actors.BaseActor actor, bool skipCallbackClear = false)
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
            hpDisplay.text = actor.Hitpoints.ToString() + "/" + actor.HitpointsMax.ToString();

            return amount;
        }

        private void Update()
        {

            if (actor.AttackCharge > 0f)
                attackGauge.text = actor.AttackCharge.ToString("0.%");
            else
                attackGauge.text = string.Empty;
            if (actor.AttackCharge >= 1f)
            {
                attackGauge.color = chargedTextColor;
                timeGaugeAtFull += Time.deltaTime;
                if (timeGaugeAtFull > 0.5f && !actor.Charging)
                {
                    attackGauge.text = string.Empty;
                }
            }
            else if (actor.Charging)
            {
                attackGauge.color = chargingTextColor;
                timeGaugeAtFull = 0f;
            }
        }
    }
}
