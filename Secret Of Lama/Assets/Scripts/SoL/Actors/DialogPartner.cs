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
        public AudioClip voice;

        public void StartDialog(DialogPartner partner)
        {
            DialogUI.Instance.Display(dialog, partner, this);
        }
    }
}
