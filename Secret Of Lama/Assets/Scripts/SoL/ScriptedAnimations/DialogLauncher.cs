using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.ScriptedAnimations
{
    public class DialogLauncher : MonoBehaviour
    {
        [Tooltip("Null uses player")]
        public DialogPartner initiator;
        [Tooltip("Null uses player")]
        public DialogPartner initiated;

        private void Start()
        {
            if (initiator == null)
                initiator = PlayerController.Instance.Actor.GetComponent<DialogPartner>();
            if (initiated == null)
                initiated = PlayerController.Instance.Actor.GetComponent<DialogPartner>();

            initiated.StartDialog(initiator);
        }
    }
}
