using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.UI
{
    [RequireComponent(typeof(CreatureActor), typeof(DialogPartner))]
    public class BecomeBossBehaviour : MonoBehaviour
    {
        private void Start()
        {
            
        }

        private void Update()
        {
            BossStatusBar.Instance.SetBoss(GetComponent<CreatureActor>());
            Destroy(this);
        }
    }
}
