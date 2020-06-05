using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.UI
{
    [CreateAssetMenu(fileName = "Dialog", menuName = "Dialog")]
    public class Dialog : ScriptableObject
    {
        [Serializable]
        public class Page
        {
            public string text;
            [Tooltip("0 is the one talked to, 1 is player char")]
            public int delivererId = 0;
        }

        public List<Page> pages;
    }
}
