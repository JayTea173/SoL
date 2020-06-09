using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.ScriptedAnimations
{
    public class DestroyAfter : MonoBehaviour
    {
        public float seconds = 1f;

        private void Start()
        {
            StartCoroutine(DestroyAfterCoroutine());
        }

        public IEnumerator DestroyAfterCoroutine()
        {
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }
    }
}
