using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL
{
    public class ActorSpawner : MonoBehaviour
    {
        public float spawnCheckInterval = 0.2f;
        public float spawnChance = 0.1f;
        public int maxActive = 3;

        private float timeElapsedSinceLastCheck = 0f;

        public Transform parentTo;
        public float spawnAreaRadius;
        public WeightedRandomPrefabList prefabs;

        private List<GameObject> activeInstances;




        private void Awake()
        {
            if (spawnCheckInterval <= 0f)
            {
                Debug.LogError("Spawn Check Interval cannot be 0 or less!");
                enabled = false;
            }

            activeInstances = new List<GameObject>();
        }

        private void Update()
        {
            if (activeInstances.Count >= maxActive)
            {
                activeInstances.RemoveAll((go) => go == null);
                return;
            }

            timeElapsedSinceLastCheck += Time.deltaTime;
            while (timeElapsedSinceLastCheck > spawnCheckInterval)
            {
                timeElapsedSinceLastCheck -= spawnCheckInterval;

                if (UnityEngine.Random.value < spawnChance)
                {
                    var prefab = prefabs.Get();
                    var go = Instantiate(prefab, parentTo);
                    go.transform.position = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * spawnAreaRadius);
                    activeInstances.Add(go);
                }
            }
        }
    }
}
