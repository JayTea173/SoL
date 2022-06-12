using SoL.Actors;
using System;
using System.Collections;
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

        public float delay;
        public bool rootOnStart;
        

        private void Start()
        {
            if (initiator == null)
                initiator = PlayerController.Instance.Actor.GetComponent<DialogPartner>();
            if (initiated == null)
                initiated = PlayerController.Instance.Actor.GetComponent<DialogPartner>();

            if (delay > 0f)
                StartCoroutine(DelayedStart());
            else
                StartDialog();

            if (rootOnStart && delay > 0f)
            {
                initiator.GetComponent<CharacterActor>().RootForSeconds(delay, false);
            }

        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(delay);
            StartDialog();
        }

        private void StartDialog()
        {


            initiated.StartDialog(initiator);
        }
    }
}
