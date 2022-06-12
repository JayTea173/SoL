using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SoL.UI
{
    public class WinOnKilled : MonoBehaviour
    {
        public string goName;

        private void Awake()
        {
            GetComponent<BaseActor>().onKilledBy += (k) =>
            {
                GameObject.Find(goName).GetComponent<CanvasGroup>().alpha = 1f;
            };
        }
    }
}
