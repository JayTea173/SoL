using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Triggers
{
    public class DisplayTipTrigger : MonoBehaviour
    {
        public string text;

        private void Awake()
        {

        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.GetComponent<CharacterActor>() == null)
                return;
            TipBehaviour.Instance.SetText(text);
            TipBehaviour.Instance.SetVisibility(true);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.GetComponent<CharacterActor>() == null)
                return;
            TipBehaviour.Instance.SetVisibility(false);
        }

    }
}
