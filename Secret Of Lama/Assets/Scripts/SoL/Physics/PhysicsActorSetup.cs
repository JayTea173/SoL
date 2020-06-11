using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Physics
{
    [RequireComponent(typeof(QuadTreeAgentBehaviour))]
    public class PhysicsActorSetup : MonoBehaviour
    {
        protected QuadTree.Agent agent;
        public SpriteRenderer spriteRenderer;

        private void Awake()
        {
            var a = GetComponent<QuadTreeAgentBehaviour>();
            agent = new Physics.QuadTree.Agent(transform, spriteRenderer.sprite.bounds, Engine.QuadTree);
            agent.ab = a;
            transform.hasChanged = true;
            
        }

        private void LateUpdate()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                Engine.QuadTree.PlaceAgent(agent);
            }
        }
    }
}
