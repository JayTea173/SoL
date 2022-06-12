using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    [CreateAssetMenu(fileName = "Audiobank", menuName = "Sound/Audio Bank", order = 0)]
    public class Soundbank : ScriptableObject
    {
        public AudioClip[] entries;

        private int l;

        private void OnEnable()
        {
            l = entries.Length;
        }

        public AudioClip GetRandom()
        {
            return entries[UnityEngine.Random.Range(0, l)];
        }
    }
}
