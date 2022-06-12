using SoL.Actors;
using SoL.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Triggers
{
    public class DialogTrigger : Trigger
    {
        public Dialog dialog;
        public DialogPartner partner;
        [Tooltip("Gameobjects that can be accessed via array index from the dialog asset")]
        public GameObject[] referencedGameObjects;

        protected override void ExecuteAction(Collider2D collider, Collision2D collision)
        {
            var p = collider.GetComponent<DialogPartner>();


            if (p != null)
            {
                dialog.referencedSceneObjects = referencedGameObjects;
                DialogUI.Instance.Display(dialog, partner == null ? p : partner, p, 0);
            }
        }
    }
}
