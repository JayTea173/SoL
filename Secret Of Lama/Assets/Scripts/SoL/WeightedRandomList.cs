using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL
{
    [Serializable]
    public class WeightedRandomList<T>
    {
        private float weightTotal = 0f;
        [SerializeField]
        protected List<Entry> entries;

        [Serializable]
        public class Entry
        {
            public T value;
            public float weight;

            public Entry(T value, float weight = 1f)
            {
                this.value = value;
                this.weight = weight;
            }
        }

        public WeightedRandomList()
        {
            entries = new List<Entry>();
        }

        public void Add(Entry e)
        {
            weightTotal += e.weight;
            entries.Add(e);
        }

        public T Get()
        {
            int c = entries.Count;
            if (c == 0)
                return default(T);
            float r = UnityEngine.Random.Range(0f, weightTotal);
            for (int i = 0; i < c; i++)
            {
                float w = entries[i].weight;
                if (r < w)
                    return entries[i].value;
                else
                    r -= w;
            }

            Debug.LogError("Weighted random executed unreachable code. Did the weightsum get modified?");
            return default(T);
        }

    }

}
