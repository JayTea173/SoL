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

        protected override void ExecuteAction(Collision2D collision)
        {
            var p = collision.collider.GetComponent<DialogPartner>();
            if (p != null)
            {
                DialogUI.Instance.Display(dialog, partner == null ? p : partner, p, 0);
            }
        }
    }
}
