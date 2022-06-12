using SoL.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class DialogPartner : MonoBehaviour
    {
        public Dialog dialog;
        public string displayName;
        public AudioClip voice;
        public bool removeDialogAfterLaunch;


        public void StartDialog(DialogPartner partner)
        {
            DialogUI.Instance.Display(dialog, partner, this);
            if (removeDialogAfterLaunch)
                dialog = null;

        }

    }
}
